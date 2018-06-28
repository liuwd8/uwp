using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Xml.Linq;
using App1.Model;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Storage;

namespace App1.Service
{
    public class TileService
    {
        public static void SetBadgeCountOnTile(int count)
        {
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());

            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        public static async void CreateTiles(Class1 item, TileUpdater updater)
        {
            string path;
            path = await stringToPath(item.picPath);
            XDocument xDoc = new XDocument(
                new XElement("tile", new XAttribute("version", 3),
                    new XElement("visual",
                        // Small Tile  
                        new XElement("binding", new XAttribute("displayName", "App1"), new XAttribute("template", "TileSmall"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", path)),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                )
                            )
                        ),

                        // Medium Tile  
                        new XElement("binding", new XAttribute("displayName", "App1"), new XAttribute("template", "TileMedium"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", path)),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                ),
                                new XElement("subgroup", new XAttribute("hint-weight", 15),
                                    new XElement("image", new XAttribute("placement", "inline"), new XAttribute("src", "Assets/preview.jpg"))
                                )
                            )
                        ),

                        // Wide Tile  
                        new XElement("binding", new XAttribute("displayName", "App1"), new XAttribute("template", "TileWide"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", path)),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                ),
                                new XElement("subgroup", new XAttribute("hint-weight", 15),
                                    new XElement("image", new XAttribute("placement", "inline"), new XAttribute("src", "Assets/preview.jpg"))
                                )
                            )
                        ),
                        
                        //Large Tile  
                        new XElement("binding", new XAttribute("displayName", "App1"), new XAttribute("template", "TileLarge"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", path)),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                ),
                                new XElement("subgroup", new XAttribute("hint-weight", 15),
                                    new XElement("image", new XAttribute("placement", "inline"), new XAttribute("src", "Assets/preview.jpg"))
                                )
                            )
                        )
                    )
                )
            );

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            Debug.WriteLine(xDoc.ToString());
            TileNotification notification = new TileNotification(xmlDoc);
            updater.Update(notification);
            //return xmlDoc;
            /*
            string title = item.title;
            string description = item.detail;
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/preview.jpg",
                                HintOverlay = 60
                            },
                            Children =
                            {
                               new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },

                                new AdaptiveText()
                                {
                                    Text = description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                            }
                        }
                    },

                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/preview.jpg",
                                HintOverlay = 60
                            },
                            Children =
                            {
                               new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },

                                new AdaptiveText()
                                {
                                    Text = description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                            }
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/preview.jpg",
                                HintOverlay = 60
                            },
                            Children =
                            {
                               new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },

                                new AdaptiveText()
                                {
                                    Text = description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                            }
                        }
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/preview.jpg",
                                HintOverlay = 60
                            },
                            Children =
                            {
                               new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },

                                new AdaptiveText()
                                {
                                    Text = description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                            }
                        }
                    }
                }
            };
            Debug.WriteLine(content.GetXml());
            return content.GetXml();*/
        }

        public static async Task<string> stringToPath(string str)
        {
            string path;
            if (str != "Assets/document.jpg")
                path = (await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(str)).Path;
            else
                path = str;
            return path;
        }

        public static void Update()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            int count = 0;
            foreach (var item in MainPage.one.AllItems)
            {
                count++;
                SetBadgeCountOnTile(MainPage.one.AllItems.Count);
                //XmlDocument xmlDoc = CreateTiles(item);
                //TileNotification notification = new TileNotification(xmlDoc);
                //updater.Update(notification);
                CreateTiles(item, updater);
                if (count == 5) break;
            }
        }
    }
}
