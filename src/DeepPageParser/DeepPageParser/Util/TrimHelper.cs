
using System;
using System.Globalization;

using DeepPageParser.Dom;

namespace DeepPageParser.Util
{
    /// <summary>
    /// Trim相关类，主要功能为修整节点内容
    /// </summary>
    internal static class TrimHelper
    {
        /// <summary>
        /// 去除a标签中不必要的节点
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="id"></param>
        /// <param name="classname"></param>
        /// <returns></returns>
        internal static bool IsTrimmedA(string tagname, string id, string classname)
        {
            if (tagname != "a")
            {
                return false;
            }

            return (classname == "hide") || (((classname.Length > 0) && ((classname.IndexOf("logo", StringComparison.OrdinalIgnoreCase) > -1) && (classname.IndexOf("logo", StringComparison.OrdinalIgnoreCase) == (classname.Length - 4)))) || ((id.Length > 0) && ((id.IndexOf("logo", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("logo", StringComparison.OrdinalIgnoreCase) == (id.Length - 4)))));
        }

        /// <summary>
        /// 去除div标签中不必要的节点
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="id"></param>
        /// <param name="classname"></param>
        /// <param name="innerTLen"></param>
        /// <returns></returns>
        internal static bool IsTrimmedDIV(string tagname, string id, string classname, int innerTLen)
        {
            if (tagname == "div")
            {
                if (classname.Length > 0)
                {
                    if (classname == "site-footer")
                    {
                        return true;
                    }
                    if (classname == "footerSection")
                    {
                        return true;
                    }
                    if (classname == "mboxdefault")
                    {
                        return true;
                    }

                    if (classname == "controls")
                    {
                        return true;
                    }

                    if (classname == "control")
                    {
                        return true;
                    }

                    if (classname == "buttons")
                    {
                        return true;
                    }

                    if (classname == "button")
                    {
                        return true;
                    }

                    if (classname == "share")
                    {
                        return true;
                    }

                    if (classname == "hidden")
                    {
                        return true;
                    }

                    if (classname == "hide")
                    {
                        return true;
                    }

                    if (classname == "left-ear")
                    {
                        return true;
                    }

                    if (classname == "right-ear")
                    {
                        return true;
                    }

                    if (classname == "ad")
                    {
                        return true;
                    }

                    if (classname.IndexOf("ad_", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }

                    if (classname.IndexOf("nocontent", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("nocontents", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("promo_holder", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("promo-component", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("comment", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("sharebar", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("share-tool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("sharetool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("articletools", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("share-article", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if ((classname.IndexOf("social", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("title", StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        return true;
                    }

                    if (classname.IndexOf("sociable", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("utilities", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("liveblog", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if ((classname.IndexOf("pagina", StringComparison.OrdinalIgnoreCase) > -1) && (innerTLen < 50))
                    {
                        return true;
                    }

                    if (classname.IndexOf("feed", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }

                    if ((classname.IndexOf("sidebar", StringComparison.OrdinalIgnoreCase) > -1) && (classname.IndexOf("sidebar", StringComparison.OrdinalIgnoreCase) == (classname.Length - 7)))
                    {
                        return true;
                    }

                    if ((classname.IndexOf("map", StringComparison.OrdinalIgnoreCase) > -1) && (classname.IndexOf("mapp", StringComparison.OrdinalIgnoreCase) <= -1) && (classname.IndexOf("map", StringComparison.OrdinalIgnoreCase) == (classname.Length - 3)))
                    {
                        return true;
                    }

                    if ((classname.IndexOf("logo", StringComparison.OrdinalIgnoreCase) > -1) && (classname.IndexOf("logo", StringComparison.OrdinalIgnoreCase) == (classname.Length - 4)))
                    {
                        return true;
                    }
                }

                if (id.Length > 0)
                {
                    if (id == "googleAd")
                    {
                        return true;
                    }

                    if (id == "sky_ad")
                    {
                        return true;
                    }

                    if (id.IndexOf("comment", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (id.IndexOf("sharebar", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (id.IndexOf("share-tool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (id.IndexOf("sharetool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (id.IndexOf("articletools", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if ((id.IndexOf("social", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("title", StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        return true;
                    }

                    if (id.IndexOf("liveblog", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if ((id.IndexOf("pagina", StringComparison.OrdinalIgnoreCase) > -1) && (innerTLen < 50))
                    {
                        return true;
                    }

                    if (id.IndexOf("feed", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }

                    if ((id.IndexOf("sidebar", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("sidebar", StringComparison.OrdinalIgnoreCase) == ((id.Length - 7) - 1)))
                    {
                        return true;
                    }

                    if ((id.IndexOf("map", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("mapp", StringComparison.OrdinalIgnoreCase) <= -1) && (id.IndexOf("map", StringComparison.OrdinalIgnoreCase) == ((id.Length - 3) - 1)))
                    {
                        return true;
                    }

                    if ((id.IndexOf("logo", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("logo", StringComparison.OrdinalIgnoreCase) == (id.Length - 4)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 去除h标签中不必要的节点
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="id"></param>
        /// <param name="classname"></param>
        /// <returns></returns>
        internal static bool IsTrimmedH(string tagname, string id, string classname)
        {
            if (((tagname != "h1") && (tagname != "h2")) && (tagname != "h3"))
            {
                return false;
            }

            return ((classname.Length > 0) && ((classname.IndexOf("logo", StringComparison.OrdinalIgnoreCase) > -1) && (classname.IndexOf("logo", StringComparison.OrdinalIgnoreCase) == (classname.Length - 4)))) || ((id.Length > 0) && ((id.IndexOf("logo", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("logo", StringComparison.OrdinalIgnoreCase) == (id.Length - 4))));
        }

        /// <summary>
        /// 去除section中不必要的节点
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="id"></param>
        /// <param name="classname"></param>
        /// <returns></returns>
        internal static bool IsTrimmedSECTION(string tagname, string id, string classname)
        {
            if (tagname != "section")
            {
                return false;
            }

            return ((classname.Length > 0) && (classname.IndexOf("comment", StringComparison.OrdinalIgnoreCase) > -1)) || ((id.Length > 0) && (id.IndexOf("comment", StringComparison.OrdinalIgnoreCase) > -1));
        }

        /// <summary>
        /// 去除表格节点中不必要的节点
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="id"></param>
        /// <param name="classname"></param>
        /// <returns></returns>
        internal static bool IsTrimmedTD(string tagname, string id, string classname)
        {
            if (tagname != "td")
            {
                return false;
            }

            return ((id.Length > 0) && (id.IndexOf("sidebar-", StringComparison.OrdinalIgnoreCase) == 0)) || ((classname.Length > 0) && (classname.IndexOf("sidebar ", StringComparison.OrdinalIgnoreCase) == 0));
        }

        /// <summary>
        /// 根据节点名去除不必要节点
        /// </summary>
        /// <param name="tagname"></param>
        /// <returns></returns>
        internal static bool IsTrimmedTag(string tagname)
        {
            if (tagname.Length <= 0)
            {
                return false;
            }

            return (tagname == "figcaption") || (tagname == "aside") || (tagname == "script") || ((tagname == "input") || ((tagname == "textarea") || ((tagname == "style") || ((tagname == "cite") || ((tagname == "iframe") || ((tagname == "noscript") || (tagname == "select")))))));
        }

        internal static bool IsTrimmedP(string tagname, string id, string classname, int innerTLen)
        {
            if (tagname !="p")
            {
                return false;
            }
            return innerTLen <= 0;
        }

        internal static bool IsTrimmedClass(string classname, int innerTLen)//去掉的是外部链接svg中的东西
        {
            //参考网址：http://www.bbc.com/sport/football/37667541
            if (classname == "icon-external-link" || classname == "inline-widget alignleft" || classname == "inline-widget alignright" || classname.Contains("content-sidebar"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 去除列表节点中不必要的节点
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="id"></param>
        /// <param name="classname"></param>
        /// <param name="innerTLen"></param>
        /// <returns></returns>
        internal static bool IsTrimmedUL(string tagname, string id, string classname, int innerTLen)
        {
            if (tagname == "ul")
            {
                if (id.Length > 0)
                {
                    if (id.IndexOf("comment", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (id.IndexOf("sharebar", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (id.IndexOf("share-tool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (id.IndexOf("sharetool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if ((id.IndexOf("social", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("title", StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        return true;
                    }

                    if ((id.IndexOf("pagina", StringComparison.OrdinalIgnoreCase) > -1) && (innerTLen < 50))
                    {
                        return true;
                    }
                }

                if (classname.Length > 0)
                {
                    if (classname.IndexOf("comment", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("sharebar", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("share-tool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if (classname.IndexOf("sharetool", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        return true;
                    }

                    if ((classname.IndexOf("social", StringComparison.OrdinalIgnoreCase) > -1) && (id.IndexOf("title", StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        return true;
                    }

                    if ((classname.IndexOf("pagina", StringComparison.OrdinalIgnoreCase) > -1) && (innerTLen < 50))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 判断是否需要修剪
        /// </summary>
        /// <param name="ele">节点</param>
        /// <returns>返回1，说明为叶节点并且不需要修剪；返回-1说明为无效节点，需要修剪；返回0，说明为枝干节点，需要对子节点进行判断</returns>
        internal static int TrimByTagFunc(WrappedNode ele)
        {
            string str = ele.TagName.ToLower(CultureInfo.InvariantCulture);
            string classname = ele.Class.ToLower(CultureInfo.InvariantCulture);
            string id = ele.Id.ToLower(CultureInfo.InvariantCulture);
            int length = ele.InnerText.Length;
            if (string.IsNullOrWhiteSpace(str))
            {
                return 1;
            }

            if (!ele.IsNodeVisible)
            {
                return -1;
            }

            if ((((IsTrimmedTag(str) || IsTrimmedSECTION(str, id, classname)) || (IsTrimmedH(str, id, classname) || IsTrimmedTD(str, id, classname))) || (IsTrimmedA(str, id, classname) || IsTrimmedUL(str, id, classname, length) || IsTrimmedP(str, id, classname, length))) || IsTrimmedDIV(str, id, classname, length) || IsTrimmedClass(classname, length))
            {
                return -1;
            }

            if (ele.Children.Count == 0)
            {
                return 1;
            }

            return 0;
        }
    }
}

