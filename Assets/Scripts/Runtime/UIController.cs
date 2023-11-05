#region Namespaces

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.Analytics;

#endregion

namespace GamesConverse.UI
{
	[DefaultExecutionOrder(200)]
	public class UIController : MonoBehaviour
	{
		#region Enumerators & Modules

		#region Enumerators

		public enum UICanvasType { Main, Login, SignUp, PasswordReset1, PasswordReset2, Language, Splash, BuyCoins, Shop, Impact, Quests, FortuneWheel, Gift, Profile, AvatarCustomization, Progression, Settings, About, AccountActivation, GameLoading };

		#endregion

		#region Modules

		[Serializable]
		public struct UICanvas
		{
			public Canvas canvas;
			public UICanvasType type;
			public bool isFloatingCanvas;
		}
		[Serializable]
		public struct UISideMenu
		{
			public Canvas canvas;
			public RectTransform transform;
			public RectTransform background;
			public float width;
			public bool IsActive => isActive;

			[SerializeField]
			internal Vector3 position;
			[SerializeField]
			internal float startPosition;
			[SerializeField]
			internal float hidePosition;
			[SerializeField]
			internal bool isActive;
		}
		[Serializable]
		public struct UIDialog
		{
			public Canvas canvas;
			public Text title;
			public Text message;
			public RectTransform oneButtonContainer;
			public Button button11;
			public Text button11Text;
			public RectTransform twoButtonsContainer;
			public Button button21;
			public Text button21Text;
			public Button button22;
			public Text button22Text;
			public RectTransform threeButtonsContainer;
			public Button button31;
			public Text button31Text;
			public Button button32;
			public Text button32Text;
			public Button button33;
			public Text button33Text;
			public RectTransform loaderContainer;
		}
		[Serializable]
		public struct UIGameLoading
		{
			public Image progressBar;
		}
		[Serializable]
		public struct UILogin
		{
			public InputField username;
			public InputField password;
		}
		[Serializable]
		public struct UIRegister
		{
			public InputField username;
			public InputField password;
			public InputField confirmPassword;
			public InputField email;
			public InputField nickname;
			public Dropdown country;
			public Dropdown favouriteGameGenre;
		}
		[Serializable]
		public struct UIPasswordReset1
		{
			public InputField email;
		}
		[Serializable]
		public struct UIPasswordReset2
		{
			public InputField newPassword;
			public InputField newPasswordConfirmation;
		}
		[Serializable]
		public struct UIAccountActivation
		{
			public InputField codeField1;
			public InputField codeField2;
			public InputField codeField3;
			public InputField codeField4;
			[HideInInspector]
			public string passwordResetEmail;
			[HideInInspector]
			public string code;
		}
		[Serializable]
		public struct UIAccount
		{
			public InputField username;
			public InputField nickname;
			public Dropdown country;
			public Dropdown favouriteGameGenre;
			public InputField oldPassword;
			public InputField newPassword;
			public InputField confirmPassword;
		}
		[Serializable]
		public struct UIUserStats
		{
			public Image[] badge;
			public Text[] nickname;
			public Text[] impact;
			public Text[] treesImpact;
			public Image[] impactProgress;
			public Text[] coins;
			public Text[] level;
			public Text[] tickets;
		}
		[Serializable]
		public struct UIMain
		{
			#region Enumerators & Modules

			#region Enumerators

			public enum GameTab { Featured, Favourite, AllGames }

			#endregion

			#region Modules

			[Serializable]
			public struct GameSlot
			{
				public RectTransform container;
				public RectTransform unlockContainer;
				public Image icon;
				public Text name;
				public Text description;
				public Text tags;
				public Text price;
				public Button hideButton;
				public Button restoreButton;
				public Button favouriteButton;
				public Button unfavouriteButton;
				public Button playUnlockButton;
				public Text playUnlockText;
			}

			#endregion

			#endregion

			#region Variables

			[HideInInspector]
			public GameTab tab;
			public RectTransform gameSlotsContainer;
			public RectTransform emptyListContainer;
			public Button[] tabButtons;
			public Dropdown genresDropdown;
			public GameSlot[] gameSlots;
			public float gameSlotHeightOffset;
			public float gameSlotHeight;

			#endregion

			#region Methods

			public void ShowTabWindow(GameTab tab)
			{
				this.tab = tab;

				Instance.RefreshMain();
			}

			#endregion
		}
		[Serializable]
		public struct UISettings
		{
			public Slider volume;
			public Text volumeText;
			public Toggle hubSoundsOn;
			public Toggle hubSoundsOff;
			public Toggle pushNotificationsOn;
			public Toggle pushNotificationsOff;
			public Text language;
		}
		[Serializable]
		public struct UIAvatarCustomization
		{
			#region Modules

			[Serializable]
			public class ImagesList
			{
				public Image[] images;
			}
			[Serializable]
			public struct ItemsGroup
			{
				public Image buttonImage;
				public Text buttonText;
				public int itemsTypeIndex;
			}
			[Serializable]
			public struct ItemSlot
			{
				public RectTransform transform;
				public Image icon;
				public Button button;
			}

			#endregion

			#region Variables

			public ImagesList[] avatarImages;
			public ItemsGroup[] itemGroups;
			public Text itemsListTitle;
			public ItemSlot[] itemSlots;
			public RectTransform itemsContentContainer;
			public Scrollbar itemsListScrollbar;
			public int[] defaultSprites;
			public float itemsGroupButtonNormalSize;
			public float itemsGroupButtonSelectedSize;
			public float itemsContainerRowSize;
			public float itemsContainerTopPadding;
			public float itemsContainerBottomPadding;
			public int itemsListColumns;
			public int currentActiveGroup;

			#endregion

			#region Methods

			public void ShowItemsGroup(int index)
			{
				currentActiveGroup = index;

				RefreshItemsList();
			}
			public void PreviousItemsGroup()
			{
				currentActiveGroup--;

				while (currentActiveGroup < 0)
					currentActiveGroup += itemGroups.Length;

				RefreshItemsList();
			}
			public void NextItemsGroup()
			{
				currentActiveGroup++;

				while (currentActiveGroup >= itemGroups.Length)
					currentActiveGroup -= itemGroups.Length;

				RefreshItemsList();
			}
			public void ChangeItem(int itemsTypeIndex)
			{
				List<Item> items = ItemsAndGamesManager.Instance.Items.FindAll(item => item.TypeIndex == itemsTypeIndex && item.IsBought());

				if (items.Count < 1)
					return;

				if (GameController.Instance.UserAvatarDetails.sprites[itemsTypeIndex] < 1)
					GameController.Instance.UserAvatarDetails.sprites[itemsTypeIndex] = items.FirstOrDefault().ID;

				int newItemIndex = items.FindIndex(item => item.ID == GameController.Instance.UserAvatarDetails.sprites[itemsTypeIndex]) + 1;

				while (newItemIndex >= items.Count)
					newItemIndex -= items.Count;

				GameController.Instance.UserAvatarDetails.sprites[itemsTypeIndex] = items[newItemIndex].ID;

				RefreshAvatarItemsType(itemsTypeIndex);
				GameController.Instance.UserAvatarDetails.SaveOnline();

			}
			public void RefreshItemsList()
			{
				foreach (ItemsGroup i_group in itemGroups)
				{
					i_group.buttonImage.rectTransform.sizeDelta = new Vector2(itemsGroupButtonNormalSize, itemsGroupButtonNormalSize);
					i_group.buttonText.gameObject.SetActive(false);
				}

				ItemsGroup group = itemGroups[currentActiveGroup];

				itemsListTitle.text = group.buttonText.text;
				group.buttonImage.rectTransform.sizeDelta = new Vector2(itemsGroupButtonSelectedSize, itemsGroupButtonSelectedSize);
				group.buttonText.gameObject.SetActive(true);
				
				List<Item> items = ItemsAndGamesManager.Instance.Items.FindAll(item => item.TypeIndex == group.itemsTypeIndex && item.IsBought() && (item.dependencies == null || item.dependencies.Length < 1 || Array.Exists(item.dependencies, d => Array.Exists(GameController.Instance.UserAvatarDetails.sprites, s => s == d))));

				for (int i = 0; i < itemSlots.Length; i++)
				{
					bool active = i < items.Count;

					itemSlots[i].transform.gameObject.SetActive(active);
					itemSlots[i].button.onClick.RemoveAllListeners();

					if (!active)
						continue;

					itemSlots[i].icon.sprite = items[i].Icon;

					static void ButtonListner(int ID, int typeIndex)
					{
						GameController.Instance.PlayUIClickSound();

						GameController.Instance.UserAvatarDetails.sprites[typeIndex] = ID;

						Instance.avatarCustomization.RefreshAvatar();

						if (!GameController.Instance.UserAvatarDetails.Save())
							Debug.LogWarning("Avatar has been changed but could not be saved!");

						GameController.Instance.UserAvatarDetails.SaveOnline();
					}

					Item item = items[i];

					itemSlots[i].button.onClick.AddListener(() => ButtonListner(item.ID, item.TypeIndex));
				}

				int listRows = items.Count / itemsListColumns;

				if (items.Count % itemsListColumns != 0)
					listRows++;

				itemsContentContainer.sizeDelta = new Vector2(itemsContentContainer.sizeDelta.x, itemsContainerTopPadding + itemsContainerRowSize * listRows + itemsContainerBottomPadding);
				itemsListScrollbar.value = 1f;
			}
			public void RefreshAvatar()
			{
				for (int i = 0; i < ItemsAndGamesManager.Instance.ItemTypes.Length; i++)
					RefreshAvatarItemsType(i);
			}

			private void RefreshAvatarItemsType(int itemsTypeIndex)
			{
				List<Item> typeItems = ItemsAndGamesManager.Instance.Items.FindAll(item => item.TypeIndex == itemsTypeIndex && item.IsBought());

				if (typeItems.Count < 1)
					return;

			find_item:
				Item currentItem = typeItems.Find(item => item.ID == GameController.Instance.UserAvatarDetails.sprites[itemsTypeIndex]);

				if (currentItem == null)
					return;

				if (currentItem.dependencies != null && currentItem.dependencies.Length > 0 && !currentItem.dependencies.Intersect(GameController.Instance.UserAvatarDetails.sprites).Any())
				{
					GameController.Instance.UserAvatarDetails.sprites[itemsTypeIndex] = currentItem.dependencyAlternative != 0 ? currentItem.dependencyAlternative : typeItems.FirstOrDefault().ID;

					goto find_item;
				}

				for (int i = 0; i < avatarImages[itemsTypeIndex].images.Length; i++)
					avatarImages[itemsTypeIndex].images[i].sprite = currentItem.Sprite;

				Instance.StartCoroutine(Instance.StartShowImpactTop4Players());
			}

			#endregion
		}
		[Serializable]
		public struct UIItemsShop
		{
			#region Modules

			[Serializable]
			public struct ItemSlot
			{
				public RectTransform transform;
				public Button buyOrWearButton;
				public RectTransform wearButton;
				public RectTransform buyButtonPrice;
				public Text buyButtonPriceText;
				public Image icon;
			}

			#endregion

			#region Variables

			public RectTransform pagesContainer;
			public RectTransform[] pages;
			public ItemSlot[] itemSlots;
			public Button prevButton;
			public Button nextButton;
			public float pagesMovementSpeed;
			public float pageWidth;
			public int pageSlotsCount;
			public int itemsTypeIndex;
			[HideInInspector]
			public int currentPageIndex;

			private float pagesOffset;
			private float pagesStartPosition;
			private int pagesCount;

			#endregion

			#region Methods

			public void NextPage()
			{
				currentPageIndex++;

				while (currentPageIndex >= pagesCount)
					currentPageIndex -= pagesCount;

				pagesOffset = -currentPageIndex * pageWidth;

				RefreshSideButtons();
			}
			public void PrevPage()
			{
				currentPageIndex--;

				while (currentPageIndex < 0)
					currentPageIndex += pagesCount;

				pagesOffset = -currentPageIndex * pageWidth;

				RefreshSideButtons();
			}
			public void RefreshItemsList()
			{
				List<Item> items = ItemsAndGamesManager.Instance.Items.FindAll(item => item.TypeIndex == Instance.itemsShop.itemsTypeIndex && item.Price > 0);

				for (int i = 0; i < itemSlots.Length; i++)
				{
					bool active = i < items.Count;

					itemSlots[i].transform.gameObject.SetActive(active);
					itemSlots[i].buyOrWearButton.onClick.RemoveAllListeners();

					if (!active)
						continue;

					Item item = items[i];

					itemSlots[i].icon.sprite = item.Sprite;
					itemSlots[i].buyButtonPriceText.text = item.Price.ToString();

					itemSlots[i].wearButton.gameObject.SetActive(item.IsBought());
					itemSlots[i].buyButtonPrice.gameObject.SetActive(!item.IsBought());
					itemSlots[i].buyOrWearButton.onClick.RemoveAllListeners();
					itemSlots[i].buyOrWearButton.onClick.AddListener(() =>
					{
						GameController.Instance.PlayUIClickSound();
						Instance.BuyOrWearItem(item);
					});
				}

				pagesCount = items.Count / pageSlotsCount;

				if (items.Count % pageSlotsCount != 0)
					pagesCount++;

				if (pagesCount > pages.Length)
				{
					pagesCount = pages.Length;

					Debug.Log("The item shop pages are not enough for the amount of items existing!");
				}

				for (int i = 0; i < pages.Length; i++)
					pages[i].transform.gameObject.SetActive(i < pagesCount);

				if (pagesStartPosition == 0f)
					pagesStartPosition = pagesContainer.transform.localPosition.x;

				RefreshSideButtons();
			}
			public void Update()
			{
				Vector3 position = pagesContainer.localPosition;

				position.x = Mathf.Lerp(position.x, pagesStartPosition + pagesOffset, Time.deltaTime * pagesMovementSpeed);
				pagesContainer.localPosition = new Vector3(position.x, position.y, position.z);
			}
			public void RefreshSideButtons()
			{
				prevButton.gameObject.SetActive(currentPageIndex > 0);
				nextButton.gameObject.SetActive(currentPageIndex < pagesCount - 1);
			}

			#endregion
		}
		[Serializable]
		public struct UIProgression
		{
			#region Modules

			[Serializable]
			public struct LevelBadge
			{
				public Sprite sprite;
				public Image badge;
				public Image lockedBadge;
				public Image lockImage;
				public int requiredLevel;
			}

			#endregion

			#region Variables

			public LevelBadge[] levelBadges;
			public Image levelProgressBar;
			public Text levelText;
			public Text nextLevelText;
			public Text levelProgressText;

			#endregion

			#region Methods

			public LevelBadge GetBadge()
			{
				int level = GameController.Instance.UserStatsDetails.level;
				LevelBadge badge = default;

				for (int i = 0; i < levelBadges.Length; i++)
				{
					if (level < levelBadges[i].requiredLevel)
						break;

					badge = levelBadges[i];
				}

				return badge;
			}

			#endregion
		}
		[Serializable]
		public struct UIImpact
		{
			#region Enumerators & Modules

			#region Enumerators

			public enum Tab { IRL, Progress, Photos }

			#endregion

			#region Modules

			[Serializable]
			public struct UIIRL
			{
				public RectTransform container;
				public Text fact1Text;
				public Text fact2Text;
				public Text fact3Text;
			}
			[Serializable]
			public struct UIProgress
			{
				#region Modules

				[Serializable]
				public struct RewardSlot
				{
					public Button button;
					public Image icon;
					public Image lockedIcon;
					public Text xpText;
					public int requiredXP;
					[Range(0f, 1f)]
					public float progress;
				}
				[Serializable]
				public struct UserSlot
				{
					public RectTransform slotContainer;
					public Image[] avatarImages;
					public Text nickname;
					public Image impactProgressBar;
				}
				public struct UserSlotDetails
				{
					public int[] avatarSprites;
					public string nickname;
					public int impact;
					public bool isPlayer;
					public DateTime lastLogin;
				}

				#endregion

				#region Variables

				public RectTransform container;
				public RewardSlot[] rewardSlots;
				public RectTransform userSlotsFailureContainer;
				public RectTransform userSlotsLoaderContainer;
				public RectTransform userSlotsListContainer;
				public RectTransform userSlotsContainer;
				public Button morePlayersButton;
				public Button lessPlayersButton;
				public Button failureRetryButton;
				public Color playerSlotNicknameColor;
				public float userSlotStartHeight;
				public float userSlotHeight;
				public UserSlot[] userSlots;
				public Image rewardsProgressBar;
				public Text weekTaskTitleText;
				[HideInInspector]
				public List<UserSlotDetails> userSlotsDetails;

				#endregion
			}
			[Serializable]
			public struct UIPhotos
			{
				public RectTransform container;
				public RectTransform showRoomContainer;
				public Button showRoomCloseButton;
				public Image showRoomPreviewImage;
				public Sprite[] sprites;
				public Image[] slots;
			}

			#endregion

			#endregion

			#region Variables

			public UIIRL irl;
			public UIProgress progress;
			public UIPhotos photos;
			public Image personalImpactBar;
			public Image globalImpactBar;
			public Image globalTreesBar;
			public Text personalProgressText;
			public Text globalProgressText;
			public Vector2 progressRange;

			#endregion

			#region Methods

			public void ShowTabWindow(Tab tab)
			{
				irl.container.gameObject.SetActive(tab == Tab.IRL);
				progress.container.gameObject.SetActive(tab == Tab.Progress);
				photos.container.gameObject.SetActive(tab == Tab.Photos);
			}
			public void OpenPhotoShowRoom(Sprite sprite)
			{
				photos.showRoomContainer.gameObject.SetActive(true);
				photos.showRoomCloseButton.onClick.RemoveAllListeners();
				photos.showRoomCloseButton.onClick.AddListener(() => Instance.impact.ClosePhotoShowroom());

				photos.showRoomPreviewImage.sprite = sprite;
			}
			public void ClosePhotoShowroom()
			{
				photos.showRoomContainer.gameObject.SetActive(false);
			}

			#endregion
		}
		[Serializable]
		public struct UIGift
		{
			#region Enumerators

			public enum GiftType { Coins, Item, XP, Ticket }

			#endregion

			#region Variables

			#region Static Variables

			public static int[] giftAmounts = new int[] { 100, 10, 20, 10, 50, 10, 20, 10, 50, 10, 20, 10, 50, 10, 10, 10, 20, 10, 10, 10, 100, 200 };

			#endregion

			#region Global Variables

			public Button gift;
			public Animator giftAnimator;
			public Button acceptButton;
			public Button discardButton;
			public RectTransform giftContainer;
			public RectTransform ticketGiftContainer;
			public RectTransform coinsGiftContainer;
			public Text coinsGiftText;
			public RectTransform itemGiftContainer;
			public RectTransform xpGiftContainer;
			public Text xpGiftText;
			[HideInInspector]
			public GiftType giftType;
			[HideInInspector]
			public int giftAmount;

			#endregion

			#endregion

			#region Methods

			public void RefreshGift()
			{
				string[] names = Enum.GetNames(typeof(GiftType));

			retry_refresh_gift:
				giftType = (GiftType)UnityEngine.Random.Range(0, names.Length);

				if (!RefreshGift(giftType))
					goto retry_refresh_gift;
			}
			public bool RefreshGift(GiftType giftType)
			{
				giftContainer.localScale = Vector3.zero;
				giftAmount = giftType == GiftType.Coins || giftType == GiftType.XP ? giftAmounts[UnityEngine.Random.Range(0, giftAmounts.Length)] : 1;

				discardButton.onClick.RemoveAllListeners();
				discardButton.onClick.AddListener(() => GameController.Instance.PlayUIClickSound());
				discardButton.onClick.AddListener(() => Instance.ShowCanvas(UICanvasType.Impact));
				discardButton.onClick.AddListener(() => Instance.ShowCanvas(UICanvasType.FortuneWheel));
				acceptButton.onClick.RemoveAllListeners();
				acceptButton.onClick.AddListener(() => GameController.Instance.PlayHubClip(GameController.Instance.giftClaimSound));
				acceptButton.onClick.AddListener(() => Instance.ShowCanvas(UICanvasType.Impact));
				acceptButton.onClick.AddListener(() => Instance.ShowCanvas(UICanvasType.FortuneWheel));

				acceptButton.interactable = true;

			change_gift_type:
				switch (giftType)
				{
					case GiftType.Coins:
						coinsGiftText.text = $"* {giftAmount}";

						acceptButton.onClick.AddListener(() => GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, Instance.gift.giftAmount));

						break;

					case GiftType.Item:
						List<Item> items = ItemsAndGamesManager.Instance.Items.FindAll(item => !item.IsBought());

						if (items.Count < 1)
						{
							giftType = GiftType.Coins;
							giftAmount = 10;

							goto change_gift_type;
						}

						int itemIndex = UnityEngine.Random.Range(0, items.Count);

						itemGiftContainer.GetComponent<Image>().sprite = items[itemIndex].Icon;

						acceptButton.onClick.AddListener(() => Instance.StartCoroutine(ItemsAndGamesManager.BuyOrWearItem(items[itemIndex], true, false)));

						break;

					case GiftType.XP:
						xpGiftText.text = $"* {giftAmount}";

						acceptButton.onClick.AddListener(() => GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, 0, Instance.gift.giftAmount));

						break;

					case GiftType.Ticket:
						acceptButton.onClick.AddListener(() => GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, 0, 0, Instance.gift.giftAmount));

						break;
				}

				ticketGiftContainer.gameObject.SetActive(giftType == GiftType.Ticket);
				coinsGiftContainer.gameObject.SetActive(giftType == GiftType.Coins);
				itemGiftContainer.gameObject.SetActive(giftType == GiftType.Item);
				xpGiftContainer.gameObject.SetActive(giftType == GiftType.XP);
				giftAnimator.SetBool("Open", false);
				gift.gameObject.SetActive(true);
				acceptButton.gameObject.SetActive(false);
				discardButton.gameObject.SetActive(false);
				gift.onClick.RemoveAllListeners();
				gift.onClick.AddListener(() =>
				{
					Instance.StartCoroutine(Instance.gift.StartOpenGift());
					Instance.gift.gift.onClick.RemoveAllListeners();
					Instance.gift.gift.onClick.AddListener(() =>
					{
						Instance.StopCoroutine(Instance.gift.StartOpenGift());
						Instance.gift.ShowButtons();
					});
				});

				return true;
			}

			private IEnumerator StartOpenGift()
			{
				while (GameController.Instance.IsHubClipPlaying())
					yield return null;

				GameController.Instance.PlayHubClip(GameController.Instance.giftShakingSound);

				yield return new WaitForSeconds(1f);

				Image giftContainerImage = giftContainer.GetComponent<Image>();

				giftAnimator.SetBool("Open", true);
				giftContainer.localScale = Vector3.zero;
				giftContainerImage.color = new Color(1f, 1f, 1f, 0f);

				while (GameController.Instance.IsHubClipPlaying())
					yield return null;

				GameController.Instance.PlayHubClip(GameController.Instance.giftOpenedSound);

				while (giftContainerImage.color.a < 1f)
				{
					Color color = giftContainerImage.color;

					color.a = Mathf.MoveTowards(color.a, 1f, Time.deltaTime);
					giftContainerImage.color = color;
					giftContainer.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, giftContainerImage.color.a);

					yield return new WaitForEndOfFrame();
				}

				ShowButtons();
			}
			private void ShowButtons()
			{
				giftContainer.GetComponent<Image>().color = Color.white;
				giftContainer.localScale = Vector3.one;

				Instance.gift.gift.gameObject.SetActive(false);
				acceptButton.gameObject.SetActive(true);
				discardButton.gameObject.SetActive(true);
			}

			#endregion
		}
		[Serializable]
		public struct UIQuests
		{
			#region Modules

			[Serializable]
			public struct QuestSlot
			{
				public RectTransform container;
				public Image icon;
				public Image doneCheck;
				public Text title;
				public Text description;
				public RectTransform progressBarContainer;
				public Image progressBar;
				public Text progressText;
				public Image rewardIcon;
				public RectTransform rewardTextContainer;
				public Text rewardText;
				public Button goButton;
				public Button claimButton;
				public Image claimButtonLight;
				public Button claimedButton;
			}

			#endregion

			#region Variables

			public QuestSlot[] slots;
			public Text emptyText;
			public Image generalBackground;
			public Image dailyBackground;
			public Image weeklyBackground;
			public Image monthlyBackground;
			public Image yearlyBackground;
			public Sprite coinIcon;
			public Sprite xpIcon;
			public Sprite levelIcon;
			public Sprite ticketIcon;
			public Sprite shareIcon;
			public Sprite rateIcon;
			public float claimButtonLightRotationSpeed;
			[HideInInspector]
			public Quest.Period tab;

			#endregion
		}
		[Serializable]
		public struct UIFortuneWheel
		{
			public Image wheel;
			public Sprite wheelSpin0;
			public Sprite wheelSpin1;
			public Sprite wheelSpin2;
			public Sprite wheelSpin5;
			public Sprite wheelSpin10;
			public Sprite wheelSpin20;
			public Sprite wheelSpin40;
			public Sprite wheelSpin60;
			public Sprite wheelSpin80;
			public Sprite wheelSpin100;
			public Image pointer;
			public Sprite pointerSpin0;
			public Sprite pointerSpin20;
			public Sprite pointerSpin40;
			public Sprite pointerSpin60;
			public Sprite pointerSpin80;
			public Sprite pointerSpin100;
			public Button spinButton;
			public Button closeButton;
			public Text ticketsText;
			public float pointerAngle;
			public float pointerStep;
			public float slot1Angle;
			public float slot2Angle;
			public float slot3Angle;
			public float slot4Angle;
			public float spinSpeed;
			public float spinDamping;
			[HideInInspector]
			public float spinTime;
			[HideInInspector]
			public float spinTimer;
			[HideInInspector]
			public float spinRotation;
			[HideInInspector]
			public float pointerLastRotation;
			[HideInInspector]
			public bool spinSoundCanBePlayed;
		}


		#endregion

		#endregion

		#region Variables

		#region Static Variables

		public static UIController Instance
		{
			get
			{
				if (!instance)
					instance = FindObjectOfType<UIController>();

				return instance;
			}
		}

		private static UIController instance;

		#endregion

		#region Global Variables

		public List<UICanvas> UICanvases = new();
		public UISideMenu sideMenu;
		public UIDialog dialog;
		public UIGameLoading gameLoading;
		public UILogin login;
		public UIRegister register;
		public UIPasswordReset1 passwordReset1;
		public UIPasswordReset2 passwordReset2;
		public UIAccountActivation accountActivation;
		public UIAccount account;
		public UIUserStats userStats;
		public UIMain main;
		public UISettings settings;
		public UIAvatarCustomization avatarCustomization;
		public UIItemsShop itemsShop;
		public UIProgression progression;
		public UIImpact impact;
		public UIGift gift;
		public UIQuests quests;
		public UIFortuneWheel fortuneWheel;

		#endregion

		#endregion

		#region Methods

		#region Start

		private void Start()
		{
			sideMenu.background.gameObject.SetActive(false);
			sideMenu.canvas.gameObject.SetActive(true);

			sideMenu.position = sideMenu.transform.anchoredPosition;
			sideMenu.startPosition = sideMenu.position.x;
			sideMenu.hidePosition = sideMenu.startPosition - sideMenu.width;
			sideMenu.position.x = sideMenu.hidePosition;
			sideMenu.transform.anchoredPosition = sideMenu.position;

			avatarCustomization.RefreshItemsList();
			itemsShop.RefreshItemsList();
			itemsShop.RefreshSideButtons();
			SelectedMainTabButton(0);
		}

		#endregion

		#region Utilities

		#region General

		public void ShowSideMenu()
		{
			sideMenu.isActive = true;
		}
		public void HideSideMenu()
		{
			sideMenu.isActive = false;
		}
		public void ShowCanvas(UICanvasType canvasType)
		{
            if (GameController.Instance.UserSessionDetails!= null && GameController.Instance.UserSessionDetails.isGuest)
            {
                switch (canvasType)
                {
                    case UICanvasType.Profile:
                    case UICanvasType.FortuneWheel:
                    case UICanvasType.Impact:
                    case UICanvasType.BuyCoins:
                        ShowDialog("Restricted Feature", "This feature is for registered users only, would you like to register now?", "Yes", "No", () => { ShowCanvas(UICanvasType.SignUp); });
                        return;
                }
            }

            UICanvas canvas = UICanvases.Find(c => c.type == canvasType);

			if (!canvas.canvas)
				return;

			if (!canvas.isFloatingCanvas)
				HideAllCanvases();

			canvas.canvas.gameObject.SetActive(true);
		}
		public void ShowCanvas(int canvasType)
		{			
			ShowCanvas((UICanvasType)canvasType);
		}
		public void HideFloatingCanvas(UICanvasType canvasType)
		{
			UICanvas canvas = UICanvases.Find(c => c.type == canvasType);

			if (!canvas.canvas || !canvas.isFloatingCanvas)
				return;

			canvas.canvas.gameObject.SetActive(false);
		}
		public void HideFloatingCanvas(int canvasType)
		{
			HideFloatingCanvas((UICanvasType)canvasType);
		}
		public void HideAllCanvases()
		{
			UICanvases.ForEach(canvas =>
			{
				if (!canvas.canvas)
					return;

				canvas.canvas.gameObject.SetActive(false);
			});
		}
		public bool TryShowInternetConnectionCheckDialog(UnityAction okayAction = null, string cancel = "Cancel", UnityAction cancelAction = null, string reconnect = "Reconnect")
		{
			if (!ItemsAndGamesManager.OfflineMode)
				return true;

			ShowDialog("Oopsie!", "Please check your internet connection...", reconnect, cancel, okayAction, cancelAction);

			return false;
		}
		public void ShowDialog(string title, string message)
		{
			SetDialogElements(title, message);
			HideDialogElements();

			dialog.loaderContainer.gameObject.SetActive(true);
		}
		public void ShowDialog(string title, string message, string okay, UnityAction okayAction = null)
		{
			GameController.Instance.PlayHubClip(GameController.Instance.dialogPopupSound, true);
			SetDialogElements(title, message);
			HideDialogElements();

			dialog.oneButtonContainer.gameObject.SetActive(true);
			dialog.button11Text.text = okay;

			if (okayAction != null)
				dialog.button11.onClick.AddListener(okayAction);
		}
		public void ShowDialog(string title, string message, string okay, string cancel, UnityAction okayAction = null, UnityAction cancelAction = null)
		{
			GameController.Instance.PlayHubClip(GameController.Instance.dialogPopupSound, true);
			SetDialogElements(title, message);
			HideDialogElements();

			dialog.twoButtonsContainer.gameObject.SetActive(true);
			dialog.button21Text.text = okay;
			dialog.button22Text.text = cancel;

			if (okayAction != null)
				dialog.button21.onClick.AddListener(okayAction);

			if (cancelAction != null)
				dialog.button22.onClick.AddListener(cancelAction);
		}
		public void ShowDialog(string title, string message, string okay, string alt, string cancel, UnityAction okayAction = null, UnityAction altAction = null, UnityAction cancelAction = null)
		{
			GameController.Instance.PlayHubClip(GameController.Instance.dialogPopupSound, true);
			SetDialogElements(title, message);
			HideDialogElements();

			dialog.threeButtonsContainer.gameObject.SetActive(true);
			dialog.button31Text.text = okay;
			dialog.button32Text.text = alt;
			dialog.button33Text.text = cancel;

			if (okayAction != null)
				dialog.button31.onClick.AddListener(okayAction);

			if (altAction != null)
				dialog.button32.onClick.AddListener(altAction);

			if (cancelAction != null)
				dialog.button33.onClick.AddListener(cancelAction);
		}
		public void HideDialog()
		{
			dialog.canvas.gameObject.SetActive(false);
		}
		public void OpenURL(string url, UnityAction postAction = null)
		{
			ShowDialog("Notice", "You're about to visit an external link!", "Continue", "Cancel", () =>
			{
				Application.OpenURL(url);
				
				if (postAction != null)
					postAction.Invoke();
			});
		}
		public void OpenURL(string url)
		{
			OpenURL(url, null);
		}
		public void VisitGooglePlayStore(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.googlePlayStoreLink, () =>
			{
				QuestsManager.Instance.Quests.FindAll(quest => quest.IsActive() && !quest.IsDone() && quest.target == Quest.Target.RateApp).ForEach(quest => quest.SetPlayerTargetValue(1));

				if (postAction != null)
					postAction.Invoke();
			});
		}
		public void VisitGooglePlayStore()
		{
			VisitGooglePlayStore(null);
		}
		public void VisitAppleAppStore(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.appleAppStoreLink, () =>
			{
				QuestsManager.Instance.Quests.FindAll(quest => quest.IsActive() && !quest.IsDone() && quest.target == Quest.Target.RateApp).ForEach(quest => quest.SetPlayerTargetValue(1));

				if (postAction != null)
					postAction.Invoke();
			});
		}
		public void VisitAppleAppStore()
		{
			VisitAppleAppStore(null);
		}
		public void VisitFacebookPage(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.facebookPageLink, postAction);
		}
		public void VisitFacebookPage()
		{
			VisitFacebookPage(null);
		}
		public void VisitInstagramPage(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.instagramPageLink, postAction);
		}
		public void VisitInstagramPage()
		{
			VisitInstagramPage(null);
		}
		public void VisitTwitterPage(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.twitterPageLink, postAction);
		}
		public void VisitTwitterPage()
		{
			VisitTwitterPage(null);
		}
		public void VisitWebsite(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.websiteLink, postAction);
		}
		public void VisitWebsite()
		{
			VisitWebsite(null);
		}
		public void ShareOnFacebook(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.facebookShareLink, () =>
			{
				QuestsManager.Instance.Quests.FindAll(quest => quest.IsActive() && !quest.IsDone() && quest.target == Quest.Target.ShareApp && quest.targetSocialMedia == Quest.TargetSocialMedia.Facebook).ForEach(quest => quest.SetPlayerTargetValue(1));

				if (postAction != null)
					postAction.Invoke();
			});
		}
		public void ShareOnFacebook()
		{
			ShareOnFacebook(null);
		}
		public void ShareOnTwitter(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.twitterShareLink, () =>
			{
				QuestsManager.Instance.Quests.FindAll(quest => quest.IsActive() && !quest.IsDone() && quest.target == Quest.Target.ShareApp && quest.targetSocialMedia == Quest.TargetSocialMedia.Twitter).ForEach(quest => quest.SetPlayerTargetValue(1));

				if (postAction != null)
					postAction.Invoke();
			});
		}
		public void ShareOnTwitter()
		{
			ShareOnTwitter(null);
		}
		public void OpenShareDialog(UnityAction postAction = null)
		{
			ShowDialog("Thanks!", "Choose where you want to share...", "Facebook", "Twitter", "Close", () => ShareOnFacebook(postAction), () => ShareOnTwitter(postAction));
		}
		public void OpenShareDialog()
		{
			OpenShareDialog(null);
		}
		public void OpenRateDialog(UnityAction postAction = null)
		{
			ShowDialog("Thanks!", "Choose where you want to rate our app...", "Play Store", "App Store", "Close", () => VisitGooglePlayStore(postAction), () => VisitAppleAppStore(postAction));
		}
		public void OpenRateDialog()
		{
			OpenRateDialog(null);
		}
		public void ReportBug(UnityAction postAction = null)
		{
			OpenURL(GameController.Instance.websiteReportLink, postAction);
		}
		public void ReportBug()
		{
			ReportBug(null);
		}

		private void SetDialogElements(string title, string message)
		{
			dialog.canvas.gameObject.SetActive(true);

			dialog.title.text = title;
			dialog.message.text = message;

			dialog.button11.onClick.RemoveAllListeners();
			dialog.button11.onClick.AddListener(() => GameController.Instance.PlayUIClickSound());
			dialog.button11.onClick.AddListener(() => HideDialog());
			dialog.button21.onClick.RemoveAllListeners();
			dialog.button21.onClick.AddListener(() => GameController.Instance.PlayUIClickSound());
			dialog.button21.onClick.AddListener(() => HideDialog());
			dialog.button22.onClick.RemoveAllListeners();
			dialog.button22.onClick.AddListener(() => GameController.Instance.PlayUIClickSound());
			dialog.button22.onClick.AddListener(() => HideDialog());
			dialog.button31.onClick.RemoveAllListeners();
			dialog.button31.onClick.AddListener(() => GameController.Instance.PlayUIClickSound());
			dialog.button31.onClick.AddListener(() => HideDialog());
			dialog.button32.onClick.RemoveAllListeners();
			dialog.button32.onClick.AddListener(() => GameController.Instance.PlayUIClickSound());
			dialog.button32.onClick.AddListener(() => HideDialog());
			dialog.button33.onClick.RemoveAllListeners();
			dialog.button33.onClick.AddListener(() => GameController.Instance.PlayUIClickSound());
			dialog.button33.onClick.AddListener(() => HideDialog());

		}
		private void HideDialogElements()
		{
			dialog.loaderContainer.gameObject.SetActive(false);
			dialog.oneButtonContainer.gameObject.SetActive(false);
			dialog.twoButtonsContainer.gameObject.SetActive(false);
			dialog.threeButtonsContainer.gameObject.SetActive(false);
		}

		#endregion

		#region Main

		public void ShowMainWindow(int window)
		{
			main.ShowTabWindow((UIMain.GameTab)window);
		}
		public void RefreshMain()
		{
			StartCoroutine(StartRefreshMain());
		}
		public void SelectedMainTabButton(int index)
		{
			foreach (Button button in main.tabButtons)
				button.image.sprite = button.spriteState.disabledSprite;

			main.tabButtons[index].image.sprite = main.tabButtons[index].spriteState.selectedSprite;
		}

		internal IEnumerator StartRefreshMain()
		{
			List<Game> games = ItemsAndGamesManager.Instance.Games.FindAll(game => main.tab == UIMain.GameTab.Featured && !game.IsHidden() || main.tab == UIMain.GameTab.Favourite && game.IsFavourite() || main.tab == UIMain.GameTab.AllGames);
			List<Dropdown.OptionData> genres = games.Select(game => game.GenreName).Distinct().Select(genre => new Dropdown.OptionData(genre)).ToList();

			genres.Insert(0, new Dropdown.OptionData("All Genres"));

			main.emptyListContainer.gameObject.SetActive(games.Count < 1);

			if (main.genresDropdown.options.Count != genres.Count)
			{
				main.genresDropdown.ClearOptions();
				main.genresDropdown.AddOptions(genres);
			}

			if (main.genresDropdown.value != 0)
				games = games.Where(game => game.GenreName == genres[main.genresDropdown.value].text).ToList();

			main.gameSlotsContainer.sizeDelta = new Vector2(main.gameSlotsContainer.sizeDelta.x, main.gameSlotHeightOffset + main.gameSlotHeight * games.Count);

			if (games.Count > main.gameSlots.Length)
				Debug.LogWarning("The games count exceeds the available game slots in main screen");

			for (int i = 0; i < main.gameSlots.Length; i++)
			{
				bool active = i < games.Count;

				main.gameSlots[i].container.gameObject.SetActive(active);

				if (!active)
					continue;

				Game game = games[i];

				main.gameSlots[i].unlockContainer.gameObject.SetActive(!game.IsBought());

				main.gameSlots[i].icon.sprite = game.Icon;
				main.gameSlots[i].name.text = game.name;
				main.gameSlots[i].description.text = game.description;
				main.gameSlots[i].tags.text = game.GenreName;
				main.gameSlots[i].price.text = game.Price.ToString();
				main.gameSlots[i].playUnlockText.text = game.IsBought() ? "Play" : "Unlock";

				main.gameSlots[i].hideButton.onClick.RemoveAllListeners();
				main.gameSlots[i].restoreButton.onClick.RemoveAllListeners();
				main.gameSlots[i].favouriteButton.onClick.RemoveAllListeners();
				main.gameSlots[i].unfavouriteButton.onClick.RemoveAllListeners();
				main.gameSlots[i].playUnlockButton.onClick.RemoveAllListeners();
				main.gameSlots[i].hideButton.onClick.AddListener(() =>
				{
					GameController.Instance.PlayUIClickSound();
					game.MakeOrRemoveHidden();
				});
				main.gameSlots[i].restoreButton.onClick.AddListener(() =>
				{
					GameController.Instance.PlayUIClickSound();
					game.MakeOrRemoveHidden();
				});
				main.gameSlots[i].favouriteButton.onClick.AddListener(() =>
				{
					GameController.Instance.PlayUIClickSound();
					game.MakeOrRemoveFavourite();
				});
				main.gameSlots[i].unfavouriteButton.onClick.AddListener(() =>
				{
					GameController.Instance.PlayUIClickSound();
					game.MakeOrRemoveFavourite();
				});
				main.gameSlots[i].playUnlockButton.onClick.AddListener(() =>
				{
					GameController.Instance.PlayUIClickSound();
					game.BuyOrPlay();
				});
				main.gameSlots[i].hideButton.gameObject.SetActive(!game.IsHidden());
				main.gameSlots[i].restoreButton.gameObject.SetActive(game.IsHidden());
				main.gameSlots[i].favouriteButton.gameObject.SetActive(!game.IsFavourite());
				main.gameSlots[i].unfavouriteButton.gameObject.SetActive(game.IsFavourite());
			}

			yield return null;
		}

		#endregion

		#region Settings

		public void RefreshSettings()
		{
			GameSettings gameSettings = GameController.Instance.Settings;

			if (!gameSettings)
				return;

			settings.volume.value = gameSettings.volume;
			settings.volumeText.text = $"{Mathf.Round(gameSettings.volume * 100f)}%";

			if (gameSettings.hubSoundsOn)
			{
				settings.hubSoundsOff.isOn = false;
				settings.hubSoundsOn.isOn = true;
			}
			else
			{
				settings.hubSoundsOn.isOn = false;
				settings.hubSoundsOff.isOn = true;
			}

			if (gameSettings.pushNotificationsOn)
			{
				settings.pushNotificationsOff.isOn = false;
				settings.pushNotificationsOn.isOn = true;
			}
			else
			{
				settings.pushNotificationsOn.isOn = false;
				settings.pushNotificationsOff.isOn = true;
			}

			settings.language.text = gameSettings.language.ToString();
		}
		public void SetLanguage(GameSettings.Language language)
		{
			GameController.Instance.Settings.language = language;

			settings.language.text = language.ToString();

			GameController.Instance.Settings.Save();
		}
		public void SetLanguage(int language)
		{
			SetLanguage((GameSettings.Language)language);
		}
		public void ChangeVolume()
		{
			GameController.Instance.Settings.volume = settings.volume.value;
			settings.volumeText.text = $"{Mathf.Round(settings.volume.value * 100f)}%";

			GameController.Instance.Settings.Save();
		}
		public void ChangeHubSounds()
		{
			GameController.Instance.Settings.hubSoundsOn = settings.hubSoundsOn.isOn;

			GameController.Instance.Settings.Save();
		}
		public void ChangePushNotifications()
		{
			GameController.Instance.Settings.pushNotificationsOn = settings.pushNotificationsOn.isOn;

			GameController.Instance.Settings.Save();
		}
		public void MoveToNextLanguage()
		{
			int count = Enum.GetNames(typeof(GameSettings.Language)).Length;

			GameController.Instance.Settings.language++;

			while ((int)GameController.Instance.Settings.language >= count)
				GameController.Instance.Settings.language -= count;

			settings.language.text = GameController.Instance.Settings.language.ToString();

			GameController.Instance.Settings.Save();
		}
		public void MoveToPreviousLanguage()
		{
			int count = Enum.GetNames(typeof(GameSettings.Language)).Length;

			GameController.Instance.Settings.language--;

			while ((int)GameController.Instance.Settings.language < 0)
				GameController.Instance.Settings.language += count;

			settings.language.text = GameController.Instance.Settings.language.ToString();

			GameController.Instance.Settings.Save();
		}

		#endregion

		#region Stats

		public void RefreshUserStats()
		{
			GameController.UserStats stats = GameController.Instance.UserStatsDetails;
			UIProgression.LevelBadge badge = progression.GetBadge();

			foreach (Image bagde in userStats.badge)
			{
				bagde.sprite = badge.sprite;
				bagde.color = stats.level > 0 ? badge.badge.color : Color.white;
			}

			foreach (Text nickname in userStats.nickname)
				nickname.text = stats.nickname;

			foreach (Text impact in userStats.impact)
				impact.text = stats.impact.ToString().PadLeft(4, '0');

			foreach (Text impact in userStats.treesImpact)
				impact.text = stats.impact.ToString();

			foreach (Image impact in userStats.impactProgress)
				impact.fillAmount = (float)stats.globalImpact / GameController.Instance.weekGlobalImpactTarget;

			foreach (Text coins in userStats.coins)
				coins.text = stats.coins.ToString().PadLeft(4, '0');

			foreach (Text level in userStats.level)
			{
				level.gameObject.SetActive(stats.level > 0);

				level.text = stats.level.ToString();
			}

			foreach (Text tickets in userStats.tickets)
				tickets.text = stats.tickets.ToString().PadLeft(2, '0');
		}

		#endregion

		#region User Session

		public void Login()
		{
			if (string.IsNullOrWhiteSpace(login.username.text) || string.IsNullOrEmpty(login.password.text))
			{
				ShowDialog("Login Failed", "Please enter a valid username and password.", "Okay");

				return;
			}

			ShowDialog("Please Wait...", "Connecting...");
			StartCoroutine(GameController.Instance.StartLogin());
		}

		public void GuestLogin()
		{
            ShowDialog("Please Wait...", "Connecting...");
            StartCoroutine(GameController.Instance.StartGuestLogin());
        }

		public void Register()
		{
			ShowDialog("Please Wait...", "Almost done...");
			StartCoroutine(GameController.Instance.StartRegister());
		}
		public void PasswordReset1()
		{
			if (string.IsNullOrWhiteSpace(passwordReset1.email.text))
			{
				ShowDialog("Reset Failed", "Please enter a valid email address.", "Okay");

				return;
			}

			ShowDialog("Please Wait...", "Connecting...");
			StartCoroutine(GameController.Instance.StartPasswordReset1());
		}
		public void PasswordReset2()
		{
			if (string.IsNullOrWhiteSpace(accountActivation.code))
			{
				ShowDialog("Reset Failed", "We've had some internal errors!", "Okay");

				return;
			}
			else if (string.IsNullOrEmpty(passwordReset2.newPassword.text) || string.IsNullOrEmpty(passwordReset2.newPasswordConfirmation.text))
			{
				ShowDialog("Reset Failed", "Please enter a valid password.", "Okay");

				return;
			}
			else if (passwordReset2.newPassword.text != passwordReset2.newPasswordConfirmation.text)
			{
				ShowDialog("Reset Failed", "The submitted passwords do not match!", "Okay");

				return;
			}

			ShowDialog("Please Wait...", "Connecting...");
			StartCoroutine(GameController.Instance.StartPasswordReset2());
		}
		public void ActivateAccount()
		{
			if (string.IsNullOrWhiteSpace(accountActivation.codeField1.text) || string.IsNullOrWhiteSpace(accountActivation.codeField2.text) ||
				string.IsNullOrWhiteSpace(accountActivation.codeField3.text) || string.IsNullOrWhiteSpace(accountActivation.codeField4.text))
			{
				ShowDialog("Confirmation Failed", "Please enter a valid confirmation code.", "Okay");

				return;
			}

			ShowDialog("Please Wait...", "Connecting...");
			StartCoroutine(GameController.Instance.StartAccountActivation());
		}
		public void Logout()
		{
			if(GameController.Instance.UserSessionDetails.isGuest)
			{				
                ShowDialog("Are you sure?", "Do you really want to logout?", "Yes", "No", () => { ShowCanvas(UICanvasType.Login); }, () => { HideDialog(); });
                return;
			}
			ShowDialog("Are you sure?", "Do you really want to logout?", "Yes", "No", () => { StartCoroutine(GameController.Instance.StartLogout()); }, () => { HideDialog(); });

		}
		public void ResendActivationCode()
		{
			ShowDialog("Please Wait...", "Resending code...");
			StartCoroutine(GameController.Instance.StartResendActivationCode());
		}
		public void UpdateAccount()
		{
			ShowDialog("Please Wait...", "Updating account...");
			StartCoroutine(GameController.Instance.StartUpdateAccount());
		}
		public void RefreshUpdateAccount()
		{
			account.username.text = GameController.Instance.UserStatsDetails.username;
			account.nickname.text = GameController.Instance.UserStatsDetails.nickname;
			account.country.value = GameController.Instance.UserStatsDetails.country;
			account.favouriteGameGenre.value = GameController.Instance.UserStatsDetails.favouriteGameGenre;

			account.country.RefreshShownValue();
			account.favouriteGameGenre.RefreshShownValue();
		}
		public void RefreshInputUpdateAccount()
		{
			if (!string.IsNullOrEmpty(account.oldPassword.text))
			{
				RefreshUpdateAccount();

				account.username.interactable = false;
				account.nickname.interactable = false;
				account.country.interactable = false;
				account.favouriteGameGenre.interactable = false;
				account.newPassword.interactable = true;
				account.confirmPassword.interactable = true;
			}
			else
			{
				account.username.interactable = true;
				account.nickname.interactable = true;
				account.country.interactable = true;
				account.favouriteGameGenre.interactable = true;
				account.newPassword.interactable = false;
				account.confirmPassword.interactable = false;
			}
		}

		#endregion

		#region Avatar Customization

		public void ShowAvatarItemsGroup(int index)
		{
			avatarCustomization.ShowItemsGroup(index);
		}
		public void PreviousAvatarItemsGroup()
		{
			avatarCustomization.PreviousItemsGroup();
		}
		public void NextAvatarItemsGroup()
		{
			avatarCustomization.NextItemsGroup();
		}
		public void ChangeAvatarItem(int typeIndex)
		{
			avatarCustomization.ChangeItem(typeIndex);
		}

		#endregion

		#region Items Shop

		public void BuyOrWearItem(Item item)
		{
			StartCoroutine(ItemsAndGamesManager.BuyOrWearItem(item, true, true));
		}
		public void PrevItemsShopPage()
		{
			itemsShop.PrevPage();
		}
		public void NextItemsShopPage()
		{
			itemsShop.NextPage();
		}

		#endregion

		#region Progression

		public void RefreshProgression()
		{
			int currentLevel = GameController.Instance.UserStatsDetails.level;
			int nextLevel = currentLevel + 1;
			int currentLevelXP = GameController.UserStats.XPFromLevel(currentLevel);
			int nextLevelXP = GameController.UserStats.XPFromLevel(nextLevel);
			int currentXP = GameController.Instance.UserStatsDetails.xp;

			progression.levelProgressBar.fillAmount = Mathf.InverseLerp(currentLevelXP, nextLevelXP, currentXP);
			progression.levelProgressText.text = $"{currentXP}/{nextLevelXP}";
			progression.levelText.text = $"Level {currentLevel}";
			progression.nextLevelText.text = nextLevel.ToString();

			foreach (UIProgression.LevelBadge levelBadge in progression.levelBadges)
			{
				if (levelBadge.requiredLevel < 1)
					continue;

				levelBadge.badge.enabled = currentLevel >= levelBadge.requiredLevel;
				levelBadge.lockedBadge.enabled = !levelBadge.badge.enabled;
				levelBadge.lockImage.enabled = !levelBadge.badge.enabled;
			}
		}

		#endregion

		#region Impact

		public void ShowImpactTab(int tab)
		{
			impact.ShowTabWindow((UIImpact.Tab)tab);
		}
		public void RefreshImpact()
		{
			float personalImpact = GameController.Instance.UserStatsDetails.impact;
			float globalImpact = GameController.Instance.UserStatsDetails.globalImpact;
			float globalProgress = Mathf.Lerp(impact.progressRange.x, impact.progressRange.y, globalImpact / GameController.Instance.weekGlobalImpactTarget);

			impact.progress.weekTaskTitleText.text = GameController.Instance.weekTaskTitle;
			impact.globalImpactBar.fillAmount = globalProgress;
			impact.globalTreesBar.fillAmount = globalImpact / GameController.Instance.weekGlobalImpactTarget;
			impact.globalProgressText.text = $"Global {globalImpact}/{GameController.Instance.weekGlobalImpactTarget}";
			impact.personalProgressText.text = "Loading...";
			impact.personalImpactBar.fillAmount = 0f;

			int xp = GameController.Instance.UserStatsDetails.xp;
			int rewardIndex = -1;

			for (int i = 0; i < impact.progress.rewardSlots.Length; i++)
			{
				if (i < 1 && xp < impact.progress.rewardSlots[i].requiredXP || i > 0 && xp >= impact.progress.rewardSlots[i - 1].requiredXP && xp < impact.progress.rewardSlots[i].requiredXP || rewardIndex < 0 && i >= impact.progress.rewardSlots.Length - 1)
					rewardIndex = i;

				bool unlocked = xp >= impact.progress.rewardSlots[i].requiredXP;

				impact.progress.rewardSlots[i].icon.gameObject.SetActive(unlocked);
				impact.progress.rewardSlots[i].lockedIcon.gameObject.SetActive(!unlocked);
				impact.progress.rewardSlots[i].button.onClick.RemoveAllListeners();

				impact.progress.rewardSlots[i].xpText.text = $"{impact.progress.rewardSlots[i].requiredXP}XP";

				if (unlocked)
				{
					Button button = impact.progress.rewardSlots[i].button;
					string prefsKey = $"REWARD_{GameController.Instance.UserStatsDetails.username}_{impact.progress.rewardSlots[i].requiredXP}XP_{GameController.Instance.weekTaskTitle.ToUpper().Replace(' ', '_')}";

					button.interactable = !PlayerPrefs.HasKey(prefsKey);

					if (button.interactable)
						button.onClick.AddListener(() =>
						{
							GameController.Instance.PlayUIClickSound();
							button.onClick.RemoveAllListeners();
							GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, 0, 0, 1);
							ShowCanvas(UICanvasType.FortuneWheel);
							PlayerPrefs.SetInt(prefsKey, 0);
						});
				}
			}

			float rewardXPMin = rewardIndex >= 1 ? impact.progress.rewardSlots[rewardIndex - 1].requiredXP : 0f;
			float rewardProgressMin = rewardIndex >= 1 ? impact.progress.rewardSlots[rewardIndex - 1].progress : 0f;
			float rewardXPMax = impact.progress.rewardSlots[rewardIndex].requiredXP;
			float rewardProgressMax = impact.progress.rewardSlots[rewardIndex].progress;

			impact.progress.rewardsProgressBar.fillAmount = Mathf.Lerp(rewardProgressMin, rewardProgressMax, Mathf.InverseLerp(rewardXPMin, rewardXPMax, xp));
			impact.irl.fact1Text.text = $"{personalImpact} trees were planted thanks to <color=\"FFB93C\">{GameController.Instance.UserStatsDetails.nickname}</color> and the entire games converse community!";

			impact.progress.userSlotsFailureContainer.gameObject.SetActive(false);
			impact.progress.userSlotsLoaderContainer.gameObject.SetActive(true);
			impact.progress.userSlotsListContainer.gameObject.SetActive(false);
			impact.progress.failureRetryButton.onClick.RemoveAllListeners();
			impact.progress.morePlayersButton.onClick.RemoveAllListeners();
			impact.progress.lessPlayersButton.onClick.RemoveAllListeners();
			impact.progress.failureRetryButton.onClick.AddListener(() =>
			{
				GameController.Instance.PlayUIClickSound();
				StartCoroutine(StartShowImpactTop4Players());
			});
			impact.progress.morePlayersButton.onClick.AddListener(() =>
			{
				GameController.Instance.PlayUIClickSound();
				StartCoroutine(StartShowImpactTop100Players());
			});
			impact.progress.lessPlayersButton.onClick.AddListener(() =>
			{
				GameController.Instance.PlayUIClickSound();
				StartCoroutine(StartShowImpactTop4Players());
			});

			StartCoroutine(StartShowImpactTop4Players());

			for (int i = 0; i < Mathf.Min(impact.photos.sprites.Length, impact.photos.slots.Length); i++)
			{
				Sprite sprite = impact.photos.sprites[i];

				impact.photos.slots[i].sprite = sprite;

				Button slotButton = impact.photos.slots[i].GetComponent<Button>();

				if (!slotButton)
					continue;

				slotButton.onClick.RemoveAllListeners();
				slotButton.onClick.AddListener(() => Instance.impact.OpenPhotoShowRoom(sprite));
			}

			if (impact.photos.sprites.Length > impact.photos.slots.Length)
				Debug.LogWarning("The impact photos number exceeds the available slots count!");
			else
				for (int i = impact.photos.sprites.Length; i < impact.photos.slots.Length; i++)
				{
					Button slotButton = impact.photos.slots[i].GetComponent<Button>();

					if (slotButton)
						slotButton.interactable = false;
				}
		}

		private void ShowImpactLeaderboardFailure()
		{
			impact.progress.userSlotsFailureContainer.gameObject.SetActive(true);
			impact.progress.userSlotsLoaderContainer.gameObject.SetActive(false);
			impact.progress.userSlotsListContainer.gameObject.SetActive(false);
			impact.progress.lessPlayersButton.gameObject.SetActive(false);
			impact.progress.morePlayersButton.gameObject.SetActive(false);
		}
		private IEnumerator StartShowImpactTop100Players()
		{
			impact.progress.userSlotsFailureContainer.gameObject.SetActive(false);
			impact.progress.userSlotsLoaderContainer.gameObject.SetActive(true);
			impact.progress.userSlotsContainer.gameObject.SetActive(false);
			impact.progress.morePlayersButton.gameObject.SetActive(false);
			impact.progress.lessPlayersButton.gameObject.SetActive(true);

			impact.progress.lessPlayersButton.interactable = false;

			RefreshPlayerList(impact.progress.userSlotsDetails.OrderByDescending(user => user.impact).ToList());

			yield return new WaitForEndOfFrame();

			impact.progress.userSlotsLoaderContainer.gameObject.SetActive(false);
			impact.progress.userSlotsListContainer.gameObject.SetActive(true);
			impact.progress.userSlotsContainer.gameObject.SetActive(true);

			impact.progress.lessPlayersButton.interactable = true;
		}
		private IEnumerator StartShowImpactTop4Players()
		{
			impact.progress.userSlotsFailureContainer.gameObject.SetActive(false);
			impact.progress.userSlotsLoaderContainer.gameObject.SetActive(true);
			impact.progress.userSlotsContainer.gameObject.SetActive(false);
			impact.progress.lessPlayersButton.gameObject.SetActive(false);
			impact.progress.morePlayersButton.gameObject.SetActive(true);

			impact.progress.morePlayersButton.interactable = false;

			yield return new WaitForEndOfFrame();

			UnityWebRequest request = UnityWebRequest.Get($"{ItemsAndGamesManager.HostURL}leaderboard?username={GameController.Instance.UserSessionDetails.username}&limit={impact.progress.userSlots.Length}");

			request.disposeCertificateHandlerOnDispose = true;
			request.disposeDownloadHandlerOnDispose = true;
			request.disposeUploadHandlerOnDispose = true;

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				ShowImpactLeaderboardFailure();
				Debug.LogError($"Leaderboard Failure\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					impact.progress.userSlotsDetails = json["users"].list.Select(user => new UIImpact.UIProgress.UserSlotDetails()
					{
						nickname = user["nickname"].str,
						impact = Convert.ToInt32(user["impact"].str),
						isPlayer = user["is_player"].b,
						lastLogin = !string.IsNullOrEmpty(user["last_login"].str) ? DateTime.Parse(user["last_login"].str) : DateTime.UtcNow,
						avatarSprites = string.IsNullOrEmpty(user["avatar"].str) ? Instance.avatarCustomization.defaultSprites.ToArray() : user["avatar"].str.Split(';').Select(id => Convert.ToInt32(id)).ToArray()
					}).Where(user => string.Compare(user.lastLogin.AddDays(GameController.Instance.activeUsersPeriodInDays).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd")) >= 0).OrderByDescending(user => user.impact).ToList();

					int activeUsers = impact.progress.userSlotsDetails.Count;
					int personalImpactTarget = (GameController.Instance.weekGlobalImpactTarget / activeUsers) + (GameController.Instance.weekGlobalImpactTarget % activeUsers);
					float personalImpact = GameController.Instance.UserStatsDetails.impact;
					float personalProgress = Mathf.Lerp(impact.progressRange.x, impact.progressRange.y, personalImpact / personalImpactTarget);

					impact.personalProgressText.text = $"Personal {personalImpact}/{personalImpactTarget}";
					impact.personalImpactBar.fillAmount = personalProgress;

					RefreshPlayerList(impact.progress.userSlotsDetails.OrderByDescending(user => user.isPlayer).ToList().GetRange(0, Mathf.Min(4, impact.progress.userSlotsDetails.Count)).OrderByDescending(user => user.impact).ToList());

					yield return new WaitForEndOfFrame();

					impact.progress.userSlotsLoaderContainer.gameObject.SetActive(false);
					impact.progress.userSlotsListContainer.gameObject.SetActive(true);
					impact.progress.userSlotsContainer.gameObject.SetActive(true);

					impact.progress.morePlayersButton.interactable = true;

					break;

				default:
					//ShowImpactLeaderboardFailure();
					//Debug.LogError($"Leaderboard Failure\r\nCode: {json["response"].str}\r\nMessage: {json["message"].str}\r\nQuery: {json["query"].str}\r\nError: {json["error"].str}");

					break;
			}
		}
		private void RefreshPlayerList(List<UIImpact.UIProgress.UserSlotDetails> details)
		{
			Vector2 containerSize = impact.progress.userSlotsContainer.sizeDelta;

			containerSize.y = details.Count * Mathf.Abs(impact.progress.userSlotHeight);
			impact.progress.userSlotsContainer.sizeDelta = containerSize;

			for (int i = 0; i < impact.progress.userSlots.Length; i++)
			{
				bool active = i < details.Count;

				impact.progress.userSlots[i].slotContainer.gameObject.SetActive(active);

				if (!active)
					continue;

				int activeUsers = impact.progress.userSlotsDetails.Count;
				int personalImpactTarget = (GameController.Instance.weekGlobalImpactTarget / activeUsers) + (GameController.Instance.weekGlobalImpactTarget % activeUsers);

				impact.progress.userSlots[i].nickname.text = $"{(details[i].isPlayer ? impact.progress.userSlotsDetails.FindIndex(detail => detail.isPlayer) + 1 : i + 1)}. {details[i].nickname}";
				impact.progress.userSlots[i].nickname.color = details[i].isPlayer ? impact.progress.playerSlotNicknameColor : Color.white;
				impact.progress.userSlots[i].impactProgressBar.fillAmount = (float)details[i].impact / personalImpactTarget;

				for (int j = 0; j < impact.progress.userSlots[i].avatarImages.Length; j++)
				{
					Item item = ItemsAndGamesManager.Instance.Items.Find(item => item.ID == details[i].avatarSprites[j]);

					if (item == null)
						continue;

					Image image = impact.progress.userSlots[i].avatarImages[j];

					image.sprite = item.Sprite;
				}
			}
		}

		#endregion

		#region Quests

		public void ShowQuestsTab(int tab)
		{
			ShowQuestsTab((Quest.Period)tab);
		}
		public void ShowQuestsTab(Quest.Period tab)
		{
			quests.tab = tab;

			RefreshQuests();
		}
		public void RefreshQuests()
		{
			List<Quest> questsList = QuestsManager.Instance.Quests.FindAll(quest => quest.period == quests.tab && quest.IsActive() && !quest.IsClaimed());

			quests.emptyText.gameObject.SetActive(questsList.Count < 1);

			if (quests.generalBackground)
				quests.generalBackground.gameObject.SetActive(quests.tab == Quest.Period.General);

			if (quests.dailyBackground)
				quests.dailyBackground.gameObject.SetActive(quests.tab == Quest.Period.Daily);

			if (quests.weeklyBackground)
				quests.weeklyBackground.gameObject.SetActive(quests.tab == Quest.Period.Weekly);

			if (quests.monthlyBackground)
				quests.monthlyBackground.gameObject.SetActive(quests.tab == Quest.Period.Monthly);

			if (quests.yearlyBackground)
				quests.yearlyBackground.gameObject.SetActive(quests.tab == Quest.Period.Yearly);

			for (int i = 0; i < quests.slots.Length; i++)
			{
				bool active = i < questsList.Count;

				quests.slots[i].container.gameObject.SetActive(active);

				if (!active)
					continue;

				Quest quest = questsList[i];
				UIQuests.QuestSlot slot = quests.slots[i];

				switch (quest.target)
				{
					case Quest.Target.PlayGame:
						slot.icon.sprite = quest.TargetGame?.Icon;

						break;

					case Quest.Target.ReachLevel:
						slot.icon.sprite = quests.levelIcon;

						break;

					case Quest.Target.CollectCoins:
						slot.icon.sprite = quests.coinIcon;

						break;

					case Quest.Target.CollectXP:
						slot.icon.sprite = quests.xpIcon;

						break;

					case Quest.Target.CollectTickets:
						slot.icon.sprite = quests.ticketIcon;

						break;

					case Quest.Target.ShareApp:
						slot.icon.sprite = quests.shareIcon;

						break;

					case Quest.Target.RateApp:
						slot.icon.sprite = quests.rateIcon;

						break;
				}

				switch (quest.reward)
				{
					case Quest.Reward.Coins:
						slot.rewardIcon.sprite = quests.coinIcon;

						break;

					case Quest.Reward.XP:
						slot.rewardIcon.sprite = quests.xpIcon;

						break;

					case Quest.Reward.Tickets:
						slot.rewardIcon.sprite = quests.ticketIcon;

						break;

					case Quest.Reward.Item:
						slot.rewardIcon.sprite = quest.RewardItem?.Icon;

						break;
				}

				slot.title.text = quest.name;
				slot.description.text = quest.description;
				slot.progressBar.fillAmount = quest.CurrentPlayerTargetProgress;
				slot.progressText.text = $"{quest.ReachedTargetValue}/{quest.targetValue}";
				slot.rewardText.text = quest.rewardAmount.ToString();

				slot.doneCheck.gameObject.SetActive(quest.IsDone());
				slot.progressBarContainer.gameObject.SetActive(quest.target != Quest.Target.PlayGame && quest.target != Quest.Target.ShareApp && quest.target != Quest.Target.RateApp);
				slot.rewardTextContainer.gameObject.SetActive(quest.reward != Quest.Reward.Item);
				slot.goButton.gameObject.SetActive(!quest.IsDone());
				slot.claimButton.gameObject.SetActive(quest.IsDone() && !quest.IsClaimed());
				slot.claimedButton.gameObject.SetActive(quest.IsClaimed());

				slot.goButton.onClick.RemoveAllListeners();
				slot.claimButton.onClick.RemoveAllListeners();
				slot.claimedButton.onClick.RemoveAllListeners();
				slot.goButton.onClick.AddListener(() => quest.Go(() =>
				{
					slot.goButton.gameObject.SetActive(false);
					slot.claimButton.gameObject.SetActive(true);
					slot.claimedButton.gameObject.SetActive(false);
				}));
				slot.claimButton.onClick.AddListener(() => quest.Claim(() =>
				{
					GameController.Instance.PlayUIClickSound();

					switch (quest.reward)
					{
						case Quest.Reward.Coins:
							GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, quest.rewardAmount);

							break;

						case Quest.Reward.XP:
							GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, 0, quest.rewardAmount);

							break;

						case Quest.Reward.Tickets:
							GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, 0, 0, quest.rewardAmount);

							break;

						case Quest.Reward.Item:
							StartCoroutine(ItemsAndGamesManager.BuyOrWearItem(quest.RewardItem, true, false));

							break;
					}

					slot.goButton.gameObject.SetActive(false);
					slot.claimButton.gameObject.SetActive(false);
					slot.claimedButton.gameObject.SetActive(true);
				}));
			}
		}

		#endregion

		#region Fortune Wheel

		public void SpinFortuneWheel()
		{
			if (GameController.Instance.UserStatsDetails.tickets < 1)
			{
				StopCoroutine(FlashOutFortuneWheelTickets());
				StartCoroutine(FlashOutFortuneWheelTickets());

				return;
			}

			StopCoroutine(StartFortuneWheelSpin());
			StartCoroutine(StartFortuneWheelSpin());
		}

		private IEnumerator StartFortuneWheelSpin()
		{
			fortuneWheel.closeButton.gameObject.SetActive(false);

			fortuneWheel.spinButton.interactable = false;
			fortuneWheel.spinTime = UnityEngine.Random.Range(GameController.Instance.minFortuneWheelSpinTime, GameController.Instance.maxFortuneWheelSpinTime);

			while (GameController.Instance.IsHubClipPlaying())
				yield return null;

			GameController.Instance.PlayHubClip(GameController.Instance.wheelSpinStartSound);

			fortuneWheel.spinRotation = 0f;

			while (fortuneWheel.spinTime > 0f)
				yield return null;

			while (Utilities.Utility.Round(fortuneWheel.spinTimer, 1u) >= .1f)
				yield return null;

			yield return new WaitForSeconds(.5f);

			float wheelEulerAngle = fortuneWheel.wheel.transform.eulerAngles.z;

			while (wheelEulerAngle > 180f)
				wheelEulerAngle -= 360f;

			while (wheelEulerAngle < -180f)
				wheelEulerAngle += 360f;

			int wheelSlotIndex;
			float targetAngle;

			if (wheelEulerAngle >= fortuneWheel.slot1Angle && wheelEulerAngle < fortuneWheel.slot2Angle)
			{
				wheelSlotIndex = 0;
				targetAngle = Utilities.Utility.Average(fortuneWheel.slot1Angle, fortuneWheel.slot2Angle);
			}
			else if (wheelEulerAngle >= fortuneWheel.slot2Angle && wheelEulerAngle < fortuneWheel.slot3Angle)
			{
				wheelSlotIndex = 1;
				targetAngle = Utilities.Utility.Average(fortuneWheel.slot2Angle, fortuneWheel.slot3Angle);
			}
			else if (wheelEulerAngle >= fortuneWheel.slot4Angle && wheelEulerAngle < fortuneWheel.slot1Angle)
			{
				wheelSlotIndex = 3;
				targetAngle = Utilities.Utility.Average(fortuneWheel.slot4Angle, fortuneWheel.slot1Angle);
			}
			else
			{
				wheelSlotIndex = 2;
				targetAngle = Utilities.Utility.Average(fortuneWheel.slot3Angle, fortuneWheel.slot4Angle + 360f);
			}

			while (targetAngle < -180f)
				targetAngle += 360f;

			while (targetAngle < -180f)
				targetAngle += 360f;

			while (wheelEulerAngle != targetAngle)
			{
				wheelEulerAngle = Mathf.MoveTowardsAngle(wheelEulerAngle, targetAngle, Time.deltaTime * fortuneWheel.spinSpeed);
				fortuneWheel.wheel.transform.eulerAngles = new Vector3(0f, 0f, wheelEulerAngle);

				yield return new WaitForEndOfFrame();
			}

			if (wheelSlotIndex == 1)
				GameController.Instance.PlayHubClip(GameController.Instance.wheelSpinFailSound, true);

			yield return new WaitForSeconds(.5f);
			yield return GameController.Instance.UserStatsDetails.StartNewAction(GameController.UserStats.ActionType.SpendingOrGain, null, null, 0, 0, -1);
			yield return new WaitForSeconds(.5f);

			switch (wheelSlotIndex)
			{
				case 0:
					gift.giftType = UIGift.GiftType.Item;
					gift.RefreshGift(gift.giftType);
					ShowCanvas(UICanvasType.Gift);

					break;

				case 2:
					gift.RefreshGift();
					ShowCanvas(UICanvasType.Gift);

					break;

				case 3:
					gift.giftType = UIGift.GiftType.Coins;
					gift.RefreshGift(gift.giftType);
					ShowCanvas(UICanvasType.Gift);

					break;
			}

			fortuneWheel.closeButton.gameObject.SetActive(true);

			fortuneWheel.spinButton.interactable = true;
		}
		private IEnumerator FlashOutFortuneWheelTickets()
		{
			fortuneWheel.ticketsText.color = Color.red;

			yield return new WaitForSeconds(.5f);

			fortuneWheel.ticketsText.color = Color.white;

			yield return new WaitForSeconds(.5f);

			fortuneWheel.ticketsText.color = Color.red;

			yield return new WaitForSeconds(.5f);

			fortuneWheel.ticketsText.color = Color.white;

			yield return new WaitForSeconds(.5f);

			fortuneWheel.ticketsText.color = Color.red;

			yield return new WaitForSeconds(.5f);

			fortuneWheel.ticketsText.color = Color.white;
		}

		#endregion

		#endregion

		#region Update

		private void Update()
		{
			SideMenu();
			itemsShop.Update();
			Quests();
			FortuneWheel();
		}
		private void SideMenu()
		{
			sideMenu.position = sideMenu.transform.anchoredPosition;

			float targetSideMenuPosition = sideMenu.IsActive ? sideMenu.startPosition : sideMenu.hidePosition;

			if (sideMenu.position.x == targetSideMenuPosition)
				return;

			sideMenu.position.x = Mathf.Lerp(sideMenu.position.x, targetSideMenuPosition, Time.deltaTime * 10f);
			sideMenu.transform.anchoredPosition = sideMenu.position;

			sideMenu.background.gameObject.SetActive(sideMenu.IsActive);
		}
		private void Quests()
		{
			foreach (UIQuests.QuestSlot slot in quests.slots)
			{
				if (!slot.claimButton.gameObject.activeInHierarchy)
					continue;

				slot.claimButtonLight.transform.Rotate(0f, 0f, Time.deltaTime * quests.claimButtonLightRotationSpeed, Space.World);
			}
		}
		private void FortuneWheel()
		{
			Sprite wheelSprite = fortuneWheel.wheelSpin0;
			Sprite pointerSprite = fortuneWheel.pointerSpin0;

			fortuneWheel.spinTimer = Mathf.Lerp(fortuneWheel.spinTimer, Mathf.Exp(fortuneWheel.spinTime) * Mathf.Clamp01(fortuneWheel.spinTime) * fortuneWheel.spinSpeed * Time.deltaTime, Time.deltaTime * fortuneWheel.spinDamping);

			if (fortuneWheel.spinTimer <= 0f)
			{
				if (fortuneWheel.wheel.sprite != wheelSprite)
					fortuneWheel.wheel.sprite = wheelSprite;

				if (fortuneWheel.pointer.sprite != pointerSprite)
					fortuneWheel.pointer.sprite = pointerSprite;

				return;
			}

			float rotationOffsetPercentage = fortuneWheel.spinTimer / 360f;

			if (rotationOffsetPercentage >= 1f)
				wheelSprite = fortuneWheel.wheelSpin100;
			else if (rotationOffsetPercentage >= .8f)
				wheelSprite = fortuneWheel.wheelSpin80;
			else if (rotationOffsetPercentage >= .6f)
				wheelSprite = fortuneWheel.wheelSpin60;
			else if (rotationOffsetPercentage >= .4f)
				wheelSprite = fortuneWheel.wheelSpin40;
			else if (rotationOffsetPercentage >= .2f)
				wheelSprite = fortuneWheel.wheelSpin20;
			else if (rotationOffsetPercentage >= .1f)
				wheelSprite = fortuneWheel.wheelSpin10;
			else if (rotationOffsetPercentage >= .05f)
				wheelSprite = fortuneWheel.wheelSpin5;
			else if (rotationOffsetPercentage >= .02f)
				wheelSprite = fortuneWheel.wheelSpin2;
			else if (rotationOffsetPercentage >= .01f)
				wheelSprite = fortuneWheel.wheelSpin1;

			if (fortuneWheel.wheel.sprite != wheelSprite)
				fortuneWheel.wheel.sprite = wheelSprite;

			fortuneWheel.wheel.transform.Rotate(0f, 0f, -fortuneWheel.spinTimer, Space.World);

			fortuneWheel.spinRotation += Mathf.Clamp(fortuneWheel.spinTimer, 0f, fortuneWheel.pointerAngle * 2f);

			if (!GameController.Instance.IsHubClipPlaying())
			{
				fortuneWheel.spinSoundCanBePlayed = fortuneWheel.spinRotation >= fortuneWheel.pointerAngle * 2f;

				if (fortuneWheel.spinSoundCanBePlayed)
				{
					GameController.Instance.PlayHubClip(GameController.Instance.wheelSpinStepSounds[UnityEngine.Random.Range(0, GameController.Instance.wheelSpinStepSounds.Length)]);

					fortuneWheel.spinSoundCanBePlayed = false;
				}
			}

			while (fortuneWheel.spinRotation >= fortuneWheel.pointerAngle * 2f)
				fortuneWheel.spinRotation -= fortuneWheel.pointerAngle * 2f;

			float pointerRotation = fortuneWheel.pointerAngle * Mathf.Clamp01(fortuneWheel.spinTimer) * (Mathf.InverseLerp(0f, fortuneWheel.pointerAngle, fortuneWheel.wheel.transform.eulerAngles.z % (fortuneWheel.pointerAngle * 2f)) - Mathf.InverseLerp(fortuneWheel.pointerAngle, fortuneWheel.pointerAngle * 2f, fortuneWheel.wheel.transform.eulerAngles.z % (fortuneWheel.pointerAngle * 2f)));
			
			fortuneWheel.pointer.transform.eulerAngles = new Vector3(0f, 0f, pointerRotation);

			float pointerRotationDeltaPercentage = Utilities.Utility.Distance(pointerRotation, fortuneWheel.pointerLastRotation) / 1080f / Time.deltaTime;

			if (pointerRotationDeltaPercentage >= 1f)
				pointerSprite = fortuneWheel.pointerSpin100;
			else if (pointerRotationDeltaPercentage >= .8f)
				pointerSprite = fortuneWheel.pointerSpin80;
			else if (pointerRotationDeltaPercentage >= .6f)
				pointerSprite = fortuneWheel.pointerSpin60;
			else if (pointerRotationDeltaPercentage >= .4f)
				pointerSprite = fortuneWheel.pointerSpin40;
			else if (pointerRotationDeltaPercentage >= .2f)
				pointerSprite = fortuneWheel.pointerSpin20;

			if (fortuneWheel.pointer.sprite != pointerSprite)
				fortuneWheel.pointer.sprite = pointerSprite;

			fortuneWheel.pointerLastRotation = pointerRotation;

			if (fortuneWheel.spinTime <= 0f)
				return;

			fortuneWheel.spinTime -= Time.deltaTime;
		}

		#endregion

		#endregion
	}
}
