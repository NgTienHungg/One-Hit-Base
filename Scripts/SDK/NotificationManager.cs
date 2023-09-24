using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Random = UnityEngine.Random;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace OneHit.Notification
{
     public class NotificationManager : MonoBehaviour
     {
          [SerializeField] private string[] notiTexts;
          [SerializeField] private string[] notiDailyTexts;

          private void Start()
          {
               CreateChannel();
          }

          private void OnApplicationPause(bool pause)
          {
               if (pause)
               {
                    SetNotification();
               }
          }

          public void Set(int id, TimeSpan delay, string title, string message, int type)
          {
               DateTime localTime = DateTime.Now.AddMinutes(delay.TotalMinutes);
               double delayLocal = 0;
               if (localTime.Hour >= 0 && localTime.Hour <= 9)
               {
                    delayLocal = (10 - localTime.Hour) * 60;
               }
               delay = delay.Add(TimeSpan.FromMinutes(delayLocal));

#if UNITY_IOS
          DateTime temp = DateTime.Now.Add(delay);
          var calendarTrigger = new iOSNotificationCalendarTrigger() {
               Year = temp.Year,
               Month = temp.Month,
               Day = temp.Day,
               Hour = temp.Hour,
               Minute = temp.Minute,
               Second = temp.Second,
               Repeats = false
          };

          var notification = new iOSNotification() {
               // You can optionally specify a custom identifier which can later be 
               // used to cancel the notification, if you don't set one, a unique 
               // string will be generated automatically.
               Identifier = id.ToString(),
               Title = title,
               Body = message,
               Subtitle = Application.productName,
               ShowInForeground = true,
               ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
               CategoryIdentifier = "category_a",
               ThreadIdentifier = "thread1",
               Trigger = calendarTrigger,
          };

          iOSNotificationCenter.ScheduleNotification(notification);
#endif

#if UNITY_ANDROID
               //        Debug.Log("SET NOTIFICATION :" + delay.ToString() + " " + title + " \n" + message + "\n" + DateTime.Now.Add(delay));
               DateTime temp = DateTime.Now.Add(delay);

               var notification = new AndroidNotification();
               notification.Title = title;
               notification.Text = message;
               notification.FireTime = temp;

               AndroidNotificationCenter.SendNotificationWithExplicitID(notification, Application.identifier, id);
#endif
          }

          /// <summary>
          /// push noti at 9 am everyday
          /// </summary>
          public void SetNotificationDailyReward()
          {
               string title = "Boom!";
               DateTime now = DateTime.Now;
               try
               {
                    for (int i = 0; i < notiTexts.Length; i++)
                    {
                         Cancel(i);
                    }
               }
               catch { Debug.Log("CANCEL ERROR 4104124"); }

#if UNITY_IOS
          var calendarTrigger = new iOSNotificationCalendarTrigger() {
               Year = now.Year,
               Month = now.Month,
               Day = now.Day,
               Hour = 9,
               Repeats = false
          };

          var notification = new iOSNotification() {
               // You can optionally specify a custom identifier which can later be 
               // used to cancel the notification, if you don't set one, a unique 
               // string will be generated automatically.
               Identifier = 10000.ToString(),
               Title = title,
               Body = "Hey, I miss u",
               Subtitle = Application.productName,
               ShowInForeground = true,
               ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
               CategoryIdentifier = "category_a",
               ThreadIdentifier = "thread1",
               Trigger = calendarTrigger,
          };
          iOSNotificationCenter.ScheduleNotification(notification);

#elif UNITY_ANDROID
               var notification = new AndroidNotification();
               notification.Title = title;
               notification.Text = notiDailyTexts[Random.Range(0, notiDailyTexts.Length)];
               notification.FireTime = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0).AddDays(1);
               AndroidNotificationCenter.SendNotificationWithExplicitID(notification, Application.identifier, 10000);
#endif
          }

          public void CancelAll()
          {
               for (int i = 0; i < 20; i++)
               {
                    Cancel(i);
                    Cancel(100 + i);
               }
          }
          public void Cancel(int id)
          {

#if UNITY_ANDROID
               AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
          for (int i = 0; i < iOSNotificationCenter.GetScheduledNotifications().Length; i++) {
               iOSNotification noti = iOSNotificationCenter.GetScheduledNotifications()[i];
               if (noti.Identifier.ToString().Equals(id + "")) {
                    Debug.Log("CANCEL: " + id + " " + noti.Identifier);
                    iOSNotificationCenter.RemoveScheduledNotification(noti.Identifier);
                    break;
               }
          }
#endif
          }

          public void SetNotification()
          {
               double[] remindPlayTimes = { 2 * 60, 24 * 60, 24 * 60 * 3, 24 * 60 * 5, 24 * 60 * 7, 24 * 60 * 14, 24 * 60 * 21, 24 * 60 * 28 };
               Debug.Log("SET NOTIFICATION ");
               try
               {
                    for (int i = 0; i < notiTexts.Length; i++)
                    {
                         Cancel(i);
                    }
               }
               catch { Debug.Log("CANCEL ERROR 4104124"); }

               string[] text1 = { "Monster Hero: Stick Makeover" };


               string[] text2 = new string[notiTexts.Length];
               for (int i = 1; i <= notiTexts.Length; i++)
               {
                    text2[i - 1] = notiTexts[i - 1];
               }
               string version = SystemInfo.operatingSystem;
               try
               {
                    if (version.Contains("4.0") || version.Contains("4.1") || version.Contains("4.2") || version.Contains("4.3") || version.Contains("4.4") ||
                        version.Contains("5.0") || version.Contains("5.1") || version.Contains("5.2"))
                    {

                    }
                    else
                    {
                         string[] iconTexts = new string[]{
                         "🎁",
                         "🤩",
                         "⏰"
                    };

                         for (int i = 1; i <= notiTexts.Length; i++)
                         {
                              text2[i - 1] = iconTexts[Random.Range(0, iconTexts.Length)] + notiTexts[i - 1] + iconTexts[Random.Range(0, iconTexts.Length)];
                         }
                    }
               }
               catch (Exception e)
               {
                    Debug.LogError(e);
               }

               for (int i = 0; i < remindPlayTimes.Length; i++)
               {
                    int type = i;
                    int index = Random.Range(0, notiTexts.Length);
                    try
                    {
                         Set(
                             i,
                             TimeSpan.FromMinutes(remindPlayTimes[i]),
                             text1[Random.Range(0, text1.Length)], text2[index],
                             type);
                    }
                    catch
                    {
                         index = Random.Range(0, notiTexts.Length);
                         Set(
                            i,
                            TimeSpan.FromMinutes(remindPlayTimes[i]),
                            text1[Random.Range(0, text1.Length)], text2[index],
                            type);
                    }
               }
          }

          //
          bool isInitChannel = false;
          public void CreateChannel()
          {
               if (isInitChannel) return;
               isInitChannel = true;
#if UNITY_ANDROID
               var channel = new AndroidNotificationChannel()
               {
                    Id = Application.identifier,
                    Name = Application.productName,
                    Importance = Importance.Default,
                    Description = "Generic notifications",
               };
               AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
          }
     }
}