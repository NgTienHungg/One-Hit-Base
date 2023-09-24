namespace OneHit
{
     public static class FirebaseEvent
     {
          #region FIXED - Đã đặt trong source code ADS (không cần thêm ở đâu nữa)
          public static readonly string AD_OPEN = "AD_OPEN";
          public static readonly string AD_NATIVE = "AD_NATIVE";
          public static readonly string AD_REWARD = "AD_REWARD";
          public static readonly string AD_INTERSTITIAL = "AD_INTERSTITIAL";
          public static readonly string PURCHASE_SUCCESS_NOADS = "PURCHASE_SUCCESS_NOADS";
          #endregion

          //----------TRACKING: Ở những nơi cần log event, gọi:
          // FirebaseManager.Instance.LogEvent(FirebaseEvent.TEN_EVENT);

          public static readonly string OPEN_SETTING = "OPEN_SETTING";
          public static readonly string START_PLAY = "START_PLAY";
          public static readonly string REPLAY = "REPLAY";
     }
}