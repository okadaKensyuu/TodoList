using System;
using System.Linq;
using System.Web;
using System.Web.Security;

using Gbook.Base.Configuration;
using Toyota.Gbook.WebSite.Common;
using Toyota.Gbook.WebSite.Common.Log;


namespace Toyota.Gbook.WebSite.Authentication.Control
{
	/// <summary>
	/// Cookie の概要の説明です。
	/// 認証チケットの確認・発行を行います。
	/// その他、認証時にCookie編集する処理を行います。
	/// </summary>
	public class Cookie
	{
		/// <summary>
		/// Cookie クラスコンストラクタ
		/// </summary>
		public Cookie()
		{
			// 処理無し
		}

		/// <summary>
		/// CheckAuthenticationTicket 関数
		/// </summary>
		/// <returns>
		///   <dl>
		///     <dt>チケット在りケース</dt>
		///     <dd>戻り値 暗号解除したFormsAuthenticationTicket
		///         Cookieから認証チケット名の値を取り出し、暗号解除を行う。
		///     </dd>
		///     
		///     <dt>チケット無しケース</dt>
		///     <dd>戻り値 null</dd>
		///   </dl>
		/// </returns>
		public static FormsAuthenticationTicket CheckAuthenticationTicket()
		{
			PCSiteTraceSource.MethodStart();

			HttpCookie AuthenticationCookie = null;
			FormsAuthenticationTicket ret = null;

			// 認証チケット名のCookieを調べる
			try
			{
				AuthenticationCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
			}
			catch (Exception ex)
			{
				AuthenticationCookie = null;
				PCSiteTraceSource.MethodFailure(ex.Message);
			}

			if (AuthenticationCookie != null)  
			{
				// Cookie値を復号化
				try	
				{
					ret = FormsAuthentication.Decrypt(AuthenticationCookie.Value);
				}
				catch (Exception ex)
				{
					// 不正チケット
					ret = null;
					PCSiteTraceSource.MethodFailure(ex.Message);
				}
			}

			PCSiteTraceSource.MethodSuccess();
			return ret;
		}

		/// <summary>
		/// GetRoles 関数
		/// </summary>
		/// <returns>
		///   <dl>
		///     <dt>ロール文字</dt>
		///     <dd>戻り値 車載機モデルのロール文字列
		///     </dd>
		///   </dl>
		/// </returns>
		private static string GetRoles()
		{
			PCSiteTraceSource.MethodStart();

			string ret = "";
			try
			{
                var dataset = HttpContext.Current.Session["Toyota.Gbook.WebSite.UserDataSet"] as
                    Toyota.Gbook.WebSite.Security.DataTransferObject.ResultCDAuthenticationUserDataSet;
                if (dataset != null)
                {
                    if (dataset.CarInformation.First().IsTConnectNavi &&
                        HttpContext.Current.Session["Toyota.Gbook.WebSite.IsTerminatedUser"] != null)
                    {
                        if ((bool)HttpContext.Current.Session["Toyota.Gbook.WebSite.IsTerminatedUser"])
                        {
                            ret = Constants.ROLE.ROLE_TCONNECT_EXIT;
                            return ret;
                        }
                        else
                        {
                            ret = Constants.ROLE.ROLE_TCONNECT;
                            return ret;
                        }
                    }
                }
			}
			catch (Exception ex)
			{
				PCSiteTraceSource.MethodFailure(ex.Message);
			}

			PCSiteTraceSource.CheckPoint("return", ret);
			PCSiteTraceSource.MethodSuccess();
			return ret;
		}

		/// <summary>
		/// TicketCookieCreate 関数
		/// </summary>
		public static void TicketCookieCreate()
		{
			PCSiteTraceSource.MethodStart();
			
			// 認証チケットの発行
			try
			{
				// ユーザーのロール文字設定
				string roles = GetRoles();
				PCSiteTraceSource.CheckPoint("roles", roles);

				// チケット作成
				FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
					1,         
					HttpContext.Current.Session[Constants.PCSiteNameSpace +".InternalMemberId"].ToString(),    
					DateTime.Now,            
					DateTime.Now.AddMinutes(Convert.ToDouble(Config.Item[Constants.PCSiteNameSpace +".FormsAuthentication.TicketTime"].ToString())),
					false,                     
					roles);                   

				// チケットのCookie暗号化
				string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
				PCSiteTraceSource.CheckPoint("encryptedTicket", encryptedTicket);
				
				// チケットのCookie生成
				HttpCookie authCookie =  new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
				authCookie.Path = FormsAuthentication.FormsCookiePath;
				HttpContext.Current.Response.Cookies.Add(authCookie);

				PCSiteTraceSource.CheckPoint("CookiesAdd", FormsAuthentication.FormsCookieName, encryptedTicket, FormsAuthentication.FormsCookiePath);
				
			}
			catch (Exception ex)
			{
				PCSiteTraceSource.MethodFailure(ex.Message);
			}
			PCSiteTraceSource.MethodSuccess();
		}

		/// <summary>
		/// UseCheckBxCookie 関数
		/// </summary>
		/// <param name="UCheckFlg">保持フラグ</param>
		/// <param name="userid">ユーザーID</param>
		public static void UseCheckBxCookie(bool UCheckFlg, string userid)
		{
			PCSiteTraceSource.MethodStart();
			
			// ユーザーIDの保持チェックボックスをCookieに反映
			HttpCookie kie;

			PCSiteTraceSource.CheckPoint("UCheckFlg", UCheckFlg.ToString());
			try
			{
				if (UCheckFlg)
				{
					// チェックフラグ
					kie = new HttpCookie("ckUSEFLG");
					kie.Value = "true";
					kie.Expires = DateTime.Now.Add(new System.TimeSpan(365, 0, 0, 0));
					HttpContext.Current.Response.Cookies.Add(kie);
					
					PCSiteTraceSource.CheckPoint("CookiesAdd", "ckUSEFLG");

					// ユーザーID
					kie = new HttpCookie("ckTCONNECTID");
					kie.Value = userid;
					kie.Expires = DateTime.Now.Add(new System.TimeSpan(365, 0, 0, 0));
					HttpContext.Current.Response.Cookies.Add(kie);

                    PCSiteTraceSource.CheckPoint("CookiesAdd", "ckTCONNECTID");
				}
				else
				{
					kie = new HttpCookie("ckUSEFLG");
					kie.Value = "";
					kie.Expires = DateTime.Now.Add(new System.TimeSpan(-1, 0, 0, 0));
					HttpContext.Current.Response.Cookies.Add(kie);

					PCSiteTraceSource.CheckPoint("CookiesRemove", "ckUSEFLG");
				}
			}
			catch (Exception ex)
			{
				PCSiteTraceSource.MethodFailure(ex.Message);
			}
			PCSiteTraceSource.MethodSuccess();
		}
	}
}
