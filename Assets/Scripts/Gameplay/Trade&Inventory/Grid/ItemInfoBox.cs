using Blessing.Gameplay.TradeAndInventory;
using TMPro;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class ItemInfoBox : MonoBehaviour
    {
        public Item CurrentItem;
        [Header("Item Info")]
        [SerializeField] private TextMeshProUGUI NameText;
        [SerializeField] private TextMeshProUGUI WeightText;
        [SerializeField] private TextMeshProUGUI InfoText;
        [Header("Traits")]
        [SerializeField] private TextMeshProUGUI TraitsText;

        private RectTransform rectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void Start()
        {
            CloseInfoBox();
        }
        public void OpenInfoBox(InventoryItem selected, Vector3 position, bool isBottom = false)
        {
            int pivotY = 1;
            if (isBottom) pivotY = 0;

            rectTransform.pivot = new Vector3(rectTransform.pivot.x, pivotY);

            gameObject.SetActive(true);
            NameText.text = selected.Item.Label;
            WeightText.text = selected.Item.Weight + " kg";

            InfoText.text = selected.Item.GetInfo();

            transform.position = new (
                position.x + BaseGrid.TileSizeWidth * selected.Width * 4 / 3,
                position.y - BaseGrid.TileSizeHeight * selected.Height / 3,
                transform.position.z
            );
        }

        public void CloseInfoBox()
        {
            gameObject.SetActive(false);
        }
    }
}
