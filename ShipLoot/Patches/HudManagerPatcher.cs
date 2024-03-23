using System.Collections;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace ShipLoot.Patches
{
    [HarmonyPatch]
    internal class HudManagerPatcher
    {
        private static GameObject _ship;
        private static GameObject _totalCounter;
        private static TextMeshProUGUI _textMesh;
        private static float _displayTimeLeft;
        private const float DisplayTime = 5f;
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.PingScan_performed))]
        private static void OnScan(HUDManager __instance, InputAction.CallbackContext context)
        {
            if (GameNetworkManager.Instance.localPlayerController == null)
                return;
            if (!context.performed || !__instance.CanPlayerScan() || __instance.playerPingingScan > -0.5f)
                return;
            // Only allow this special scan to work while inside the ship.
            if (!StartOfRound.Instance.inShipPhase && !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
                return;
            
            if (!_ship)
                _ship = GameObject.Find("/Environment/HangarShip");
            if (!_totalCounter)
                CopyValueCounter();
            float value = CalculateLootValue();
            _textMesh.text = $"SHIP: ${value:F0}";
            _displayTimeLeft = DisplayTime;
            if (!_totalCounter.activeSelf)
                GameNetworkManager.Instance.StartCoroutine(ShipLootCoroutine());
        }

        private static IEnumerator ShipLootCoroutine()
        {
            _totalCounter.SetActive(true);
            while (_displayTimeLeft > 0f)
            {
                float time = _displayTimeLeft;
                _displayTimeLeft = 0f;
                yield return new WaitForSeconds(time);
            }
            _totalCounter.SetActive(false);
        }

        /// <summary>
        /// Calculate the value of all scrap in the ship.
        /// </summary>
        /// <returns>The total scrap value.</returns>
        private static float CalculateLootValue()
        {
            // Get all objects that can be picked up from inside the ship. Also remove items which technically have
            // scrap value but don't actually add to your quota.
            var loot = _ship.GetComponentsInChildren<GrabbableObject>()
                .Where(obj => obj.itemProperties.isScrap && !(obj is RagdollGrabbableObject))
                .ToList();
            ShipLoot.Log.LogDebug("Calculating total ship scrap value.");
            loot.Do(scrap => ShipLoot.Log.LogDebug($"{scrap.name} - ${scrap.scrapValue}"));
            return loot.Sum(scrap => scrap.scrapValue);
        }
        
        /// <summary>
        /// Copy an existing object loaded by the game for the display of ship loot and put it in the right position.
        /// </summary>
        private static void CopyValueCounter()
        {
            GameObject valueCounter = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
            if (!valueCounter)
                ShipLoot.Log.LogError("Failed to find ValueCounter object to copy!");
            _totalCounter = Object.Instantiate(valueCounter.gameObject, valueCounter.transform.parent, false);
            _totalCounter.transform.Translate(0f, 1f, 0f);
            Vector3 pos = _totalCounter.transform.localPosition;
            _totalCounter.transform.localPosition = new Vector3(pos.x + 50f, -50f, pos.z);
            _textMesh = _totalCounter.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
}