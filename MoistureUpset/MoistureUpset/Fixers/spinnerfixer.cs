using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MoistureUpset.Fixers
{
    class spinnerfixer : MonoBehaviour
    {
        public float scale = 1;
        public float pos = 4;
        private RoR2.PurchaseInteraction[] _purchaseInteractions;
        private RoR2.Hologram.HologramProjector _hologramProjector;

        void Start()
        {
            transform.localScale = new Vector3(scale, scale, scale);
            //transform.eulerAngles = Vector3.zero;
            _hologramProjector = GetComponentInChildren<RoR2.Hologram.HologramProjector>();
            if (_hologramProjector && _hologramProjector.hologramPivot)
            {
                _hologramProjector.hologramPivot.localPosition = new Vector3(0, pos, 0);
            }

            _purchaseInteractions = GetComponentsInChildren<RoR2.PurchaseInteraction>(true);
        }

        void Update()
        {
            if (_purchaseInteractions == null || _purchaseInteractions.Length == 0)
            {
                _purchaseInteractions = GetComponentsInChildren<RoR2.PurchaseInteraction>(true);
                if (_purchaseInteractions == null || _purchaseInteractions.Length == 0)
                {
                    return;
                }
            }

            // MultiShopController.available became inaccessible; infer availability from child terminals.
            bool anyAvailable = false;
            for (int i = 0; i < _purchaseInteractions.Length; i++)
            {
                var purchaseInteraction = _purchaseInteractions[i];
                if (purchaseInteraction && purchaseInteraction.available)
                {
                    anyAvailable = true;
                    break;
                }
            }

            if (!anyAvailable)
            {
                transform.Rotate(new Vector3(0f, 1000f * Time.deltaTime, 0f));
            }
        }
    }

    class terminalfixer : MonoBehaviour
    {
        public Vector3 center = Vector3.zero;
        void Update()
        {
            transform.RotateAround(center, new Vector3(0, 1, 0), 1000 * Time.deltaTime);
        }
    }

}
