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
	/// Cookie �̊T�v�̐����ł��B
	/// �F�؃`�P�b�g�̊m�F�E���s���s���܂��B
	/// ���̑��A�F�؎���Cookie�ҏW���鏈�����s���܂��B
	/// </summary>
	public class Cookie
	{
		/// <summary>
		/// Cookie �N���X�R���X�g���N�^
		/// </summary>
		public Cookie()
		{
			// ��������
		}

		/// <summary>
		/// CheckAuthenticationTicket �֐�
		/// </summary>
		/// <returns>
		///   <dl>
		///     <dt>�`�P�b�g�݂�P�[�X</dt>
		///     <dd>�߂�l �Í���������FormsAuthenticationTicket
		///         Cookie����F�؃`�P�b�g���̒l�����o���A�Í��������s���B
		///     </dd>
		///     
		///     <dt>�`�P�b�g�����P�[�X</dt>
		///     <dd>�߂�l null</dd>
		///   </dl>
		/// </returns>
		public static FormsAuthenticationTicket CheckAuthenticationTicket()
		{
			PCSiteTraceSource.MethodStart();

			HttpCookie AuthenticationCookie = null;
			FormsAuthenticationTicket ret = null;

			// �F�؃`�P�b�g����Cookie�𒲂ׂ�
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
				// Cookie�l�𕜍���
				try	
				{
					ret = FormsAuthentication.Decrypt(AuthenticationCookie.Value);
				}
				catch (Exception ex)
				{
					// �s���`�P�b�g
					ret = null;
					PCSiteTraceSource.MethodFailure(ex.Message);
				}
			}

			PCSiteTraceSource.MethodSuccess();
			return ret;
		}

		/// <summary>
		/// GetRoles �֐�
		/// </summary>
		/// <returns>
		///   <dl>
		///     <dt>���[������</dt>
		///     <dd>�߂�l �ԍڋ@���f���̃��[��������
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
		/// TicketCookieCreate �֐�
		/// </summary>
		public static void TicketCookieCreate()
		{
			PCSiteTraceSource.MethodStart();
			
			// �F�؃`�P�b�g�̔��s
			try
			{
				// ���[�U�[�̃��[�������ݒ�
				string roles = GetRoles();
				PCSiteTraceSource.CheckPoint("roles", roles);

				// �`�P�b�g�쐬
				FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
					1,         
					HttpContext.Current.Session[Constants.PCSiteNameSpace +".InternalMemberId"].ToString(),    
					DateTime.Now,            
					DateTime.Now.AddMinutes(Convert.ToDouble(Config.Item[Constants.PCSiteNameSpace +".FormsAuthentication.TicketTime"].ToString())),
					false,                     
					roles);                   

				// �`�P�b�g��Cookie�Í���
				string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
				PCSiteTraceSource.CheckPoint("encryptedTicket", encryptedTicket);
				
				// �`�P�b�g��Cookie����
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
		/// UseCheckBxCookie �֐�
		/// </summary>
		/// <param name="UCheckFlg">�ێ��t���O</param>
		/// <param name="userid">���[�U�[ID</param>
		public static void UseCheckBxCookie(bool UCheckFlg, string userid)
		{
			PCSiteTraceSource.MethodStart();
			
			// ���[�U�[ID�̕ێ��`�F�b�N�{�b�N�X��Cookie�ɔ��f
			HttpCookie kie;

			PCSiteTraceSource.CheckPoint("UCheckFlg", UCheckFlg.ToString());
			try
			{
				if (UCheckFlg)
				{
					// �`�F�b�N�t���O
					kie = new HttpCookie("ckUSEFLG");
					kie.Value = "true";
					kie.Expires = DateTime.Now.Add(new System.TimeSpan(365, 0, 0, 0));
					HttpContext.Current.Response.Cookies.Add(kie);
					
					PCSiteTraceSource.CheckPoint("CookiesAdd", "ckUSEFLG");

					// ���[�U�[ID
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
