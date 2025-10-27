using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.Upgrades;

public class UpgradeButtonBehaviour : MonoBehaviour
{
    [SerializeField] UpgradeType upgradeType;

    [Space]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Image iconImage;
    [SerializeField] Image backgroundImage;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] GameObject maxObject;

    [Space]
    [SerializeField] GameObject activationStateObject;
    [SerializeField] GameObject tutorialObject;

    [Space]
    [SerializeField] Color backgroundDefaultColor = Color.white;
    [SerializeField] Color backgroundMaxColor = Color.yellow;

    private Button button;
    private BaseUpgrade baseUpgrade;

    private string titleFormatText;

    private UIMainMenu mainMenu;

    private RectTransform rectTransform;
    private Vector2 defaultAnchoredPosition;

    private float currentMultiplier;
    private int multipliedPrice;

    public void Initialise(UIMainMenu mainMenu)
    {
        this.mainMenu = mainMenu;

        rectTransform = (RectTransform)transform;
        button = GetComponent<Button>();
        baseUpgrade = UpgradesController.GetUpgrade<BaseUpgrade>(upgradeType);

        defaultAnchoredPosition = rectTransform.anchoredPosition;
        titleFormatText = titleText.text;

        UpgradeUI();

        if (!baseUpgrade.IsMaxedOut)
        {
            BaseUpgradeStage nextStage = baseUpgrade.NextStage;
            if (nextStage != null)
            {
                CurrenciesController.GetCurrency(nextStage.CurrencyType).OnCurrencyChanged += OnCurrencyAmountChanged;
            }
        }
        
    }

    public void PlayAnimation(float delay)
    {
        UpgradeUI();

        rectTransform.anchoredPosition = defaultAnchoredPosition.AddToY(-1200);
        rectTransform.DOAnchoredPosition(defaultAnchoredPosition, 0.4f, delay).SetEasing(Ease.Type.CubicOut);
    }

    private void OnCurrencyAmountChanged(Currency currency, int amountDifference)
    {
        if (!baseUpgrade.IsMaxedOut)
        {
            BaseUpgradeStage nextStage = baseUpgrade.NextStage;
            if (nextStage != null)
            {
                if (CurrenciesController.Get(currency.CurrencyType) >= multipliedPrice)
                {
                    button.interactable = true;
                    activationStateObject.SetActive(true);
                    iconImage.sprite = baseUpgrade.Icon;

                    return;
                }
                
            }

            button.interactable = false;
            activationStateObject.SetActive(false);
            iconImage.sprite = baseUpgrade.DisabledIcon;
        }
    }

    public void UpgradeUI()
    {
        //currentMultiplier = LevelController.CurrentWorld.MoneyMultiplier;

        if (!baseUpgrade.IsMaxedOut)
        {
            BaseUpgradeStage nextStage = baseUpgrade.NextStage;
            if (nextStage != null)
            {
                multipliedPrice = Mathf.RoundToInt(nextStage.Price);

                titleText.text = string.Format(titleFormatText, baseUpgrade.Title.ToUpper(), baseUpgrade.UpgradeLevel + 1);

                priceText.gameObject.SetActive(true);
                priceText.text = multipliedPrice.ToString();

                backgroundImage.color = backgroundDefaultColor;

                maxObject.SetActive(false);

                if (CurrenciesController.Get(nextStage.CurrencyType) >= multipliedPrice)
                {
                    button.interactable = true;
                    activationStateObject.SetActive(true);

                    iconImage.sprite = baseUpgrade.Icon;
                }
                else
                {
                    button.interactable = false;
                    activationStateObject.SetActive(false);

                    iconImage.sprite = baseUpgrade.DisabledIcon;
                }

                return;
            }
        }

        titleText.text = string.Format(titleFormatText, baseUpgrade.Title.ToUpper(), baseUpgrade.UpgradeLevel + 1);
        priceText.gameObject.SetActive(false);

        button.interactable = true;
        activationStateObject.SetActive(false);
        iconImage.sprite = baseUpgrade.Icon;
        backgroundImage.color = backgroundMaxColor;

        maxObject.SetActive(true);
    }

    public void OnButtonClicked()
    {
        if(!baseUpgrade.IsMaxedOut)
        {
            BaseUpgradeStage nextStage = baseUpgrade.NextStage;
            if(nextStage != null)
            {
                int currencyCount = CurrenciesController.Get(nextStage.CurrencyType);
                if(currencyCount >= multipliedPrice)
                {
                    CurrenciesController.Substract(nextStage.CurrencyType, multipliedPrice);

                    baseUpgrade.UpgradeStage();

                    transform.localScale = Vector3.one * 0.8f;
                    transform.DOScale(Vector3.one, 0.3f).SetEasing(Ease.Type.BackOut);

                    UpgradeUI();

                    //mainMenu.OnUpgradeIsUpgraded();
                }
            }

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
    }

    public bool CanBeUpgraded()
    {
        if (!gameObject.activeSelf)
            return false;

        if (!baseUpgrade.IsMaxedOut)
        {
            BaseUpgradeStage nextStage = baseUpgrade.NextStage;
            if (nextStage != null)
            {
                return CurrenciesController.Get(nextStage.CurrencyType) >= multipliedPrice;
            }
        }

        return false;
    }

    public void SetTutorialState(bool state)
    {
        tutorialObject.SetActive(state);
    }
}
