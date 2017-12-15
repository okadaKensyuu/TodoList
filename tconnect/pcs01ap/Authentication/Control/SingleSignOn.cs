using Gbook.Base.Configuration;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Toyota.Gbook.WebSite.Common.Log;

namespace Toyota.Gbook.WebSite.Authentication.Control
{
	/// <summary>
	/// SingleSignOn�N���X�ł��B
	/// </summary>
	public class SingleSignOn
	{
        #region // �V���O���T�C���I���C���^�[�t�F�[�X�ŗ��p����萔
        /// <summary>
        ///	�Í����L�[�̔z��̃C���f�b�N�X
        /// </summary>
        public const int EncryptTicketKeyIndex = 32;

        /// <summary>
        ///	�Í��������x�N�^�[�̔z��̃C���f�b�N�X
        /// </summary>
        public const int EncryptTicketIVIndex = 16;
        #endregion
        
		/// <summary>
		///	�R���X�g���N�^
		/// </summary>
		public SingleSignOn()
		{
		}

		/// <summary>
        /// URL����
		/// </summary>
        /// <remarks>
        /// <para>���̊֐��́AvalidationUrl + "?targeturl=" + targetUrl + "&amp;ticket=" + key �̌`����URL������𐶐����܂��B</para>
        /// <para>key�͈Í�������AQueryString�Ƃ��ĕt�^����܂��B</para>
        /// </remarks>
        /// <param name="validationUrl">���ؐ�URL</param>
        /// <param name="targetUrl">�J�ڐ�URL</param>
		/// <param name="key">�L�[</param>
		/// <returns>�F�؃L�[���܂ރ��_�C���N�g��URL</returns>
		public string GenerateURL(string validationUrl, string targetUrl, string key)
		{
			//	���\�b�h�J�n���L�^���܂��B
			PCSiteTraceSource.MethodStart(validationUrl, targetUrl, key);

            // ���ؐ�y�[�WURL�̃`�F�b�N�inull�A�󔒃`�F�b�N�j
            if (string.IsNullOrEmpty(validationUrl))
            {
                throw new ArgumentNullException("validationUrl");
            }
			// �J�ڐ�y�[�W���ʎq�̃`�F�b�N�inull�A�󔒃`�F�b�N�j
            if (string.IsNullOrEmpty(targetUrl))
			{
                throw new ArgumentNullException("targetUrl");
			}
			// �L�[�l�̃`�F�b�N�inull�A�󔒃`�F�b�N�j
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}

			try
			{
				string returnURL = "";
			
				PCSiteTraceSource.CheckPoint("GenerateURL���\�b�h�`�F�b�N(�^�[�Q�b�gURL)�F" + targetUrl);

				//	SingleSignOnTicket�N���X��Gentrate���\�b�h�Ăяo��
				string encriptTicket = SingleSignOnAuthKey.Generate(key); 

				PCSiteTraceSource.CheckPoint("GenerateURL���\�b�h�`�F�b�N(�Í����`�P�b�g)�F" + encriptTicket);

				// �F�؃L�[���܂ރ��_�C���N�g��URL�쐬
				StringBuilder sb = new StringBuilder();
				sb.Append(validationUrl);
				sb.Append("?targeturl=");

				// target�̒l��URL�G���R�[�h���܂��B
                sb.Append(System.Web.HttpUtility.UrlEncode(targetUrl));
				sb.Append("&ticket=");

				//	 ase64�ŃG���R�[�h�������Ɂu+�A/�v�����邽��URL�G���R�[�h���܂��B
				sb.Append(System.Web.HttpUtility.UrlEncode(encriptTicket));
				returnURL = sb.ToString();
				
				PCSiteTraceSource.CheckPoint("GenerateURL���\�b�h�`�F�b�N(ReturnUrl)�F" + returnURL);

				//	���\�b�h����I�����L�^���܂��B
				PCSiteTraceSource.MethodSuccess();
				return returnURL;
			}
			catch (Exception ex)
			{
				//	�A�v���P�[�V�����G���[���g���[�X���܂��B
				PCSiteTraceSource.AppError("SingleSignOn.GenerateURL���Ăяo�����ۂ�Exception", validationUrl, targetUrl, key, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}

		/// <summary>
		///	�Í���<br/>
		///	Rijndael �A���S���Y�����g�p<br/>
		/// </summary>
		/// <param name="st">�F�؃L�[</param>
		/// <returns>�Í������ꂽ�F�؃L�[</returns>
		internal static string EncryptString(string st)
		{
			//	�`�P�b�g�̃`�F�b�N�inull�A�󔒃`�F�b�N�j
			if (st == null || st == "")
			{
				throw new ArgumentNullException("Key");
			}
			
			//	���\�b�h�J�n���L�^���܂��B
			PCSiteTraceSource.MethodStart(st);

			try
			{
				// �Í������ꂽ�F�؃L�[
				string encryptionTicket;

				byte[] key = null;
				byte[] iv = null;

				// �Í����L�[���쐬���܂��B
				GenerateKey(ref key, ref iv);
		 
				// ���͕������ byte �z��ɕϊ����܂��B
				ASCIIEncoding encoding = new ASCIIEncoding();
				byte[] source = encoding.GetBytes(st);

				// Rijndael �̃T�[�r�X �v���o�C�_�𐶐����܂��B
                using (RijndaelManaged myRijndael = new RijndaelManaged())
                {
                    // ���o�͗p�̃X�g���[���𐶐����܂��B
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, myRijndael.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                        {
                            // �X�g���[���ɈÍ�������f�[�^���������݂܂��B
                            cs.Write(source, 0, source.Length);
                            cs.Close();

                            // �Í������ꂽ�f�[�^�� byte �z��Ŏ擾���܂��B
                            byte[] destination = ms.ToArray();
                            ms.Close();

                            // byte �z��𕶎���ɕϊ����ĕ\�����܂��B
                            encryptionTicket = Convert.ToBase64String(destination);
                        }
                    }
                }
				// ���\�b�h����I�����L�^���܂��B
				PCSiteTraceSource.MethodSuccess();
				return encryptionTicket;
			}
			catch (Exception ex)
			{
				// �A�v���P�[�V�����G���[���g���[�X���܂��B
				PCSiteTraceSource.AppError("SingleSignOn.EncryptString���Ăяo�����ۂ�Exception", st, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}
		
		/// <summary>
		///	������<br/>
		///	Rijndael �A���S���Y�����g�p<br/>
		/// </summary>
		/// <param name="st">�Í������ꂽ�F�؃L�[</param>
		/// <returns>���������ꂽ�F�؃L�[</returns>
		internal static string DecryptString(string st)
		{
			// �p�����[�^�̃`�F�b�N�inull�A�󔒃`�F�b�N�j
			if (st == null || st == "")
			{
				throw new ArgumentNullException("Ticket");
			}

			// ���\�b�h�J�n���L�^���܂��B
			PCSiteTraceSource.MethodStart(st);

			try
			{
				// �������F�؃L�[
				string decryptionTicket;

				// �p�����[�^��null�A�󔒃`�F�b�N
				if(st == null || st == "")
				{
					throw new ArgumentNullException("st");
				}

				byte[] key = null;
				byte[] iv = null;
				
				// �Í����L�[���쐬���܂��B
				GenerateKey(ref key, ref iv);

				// �������Bese64�Ńf�R�[�h����byte �z��ɕϊ����܂��B
				byte[] source = Convert.FromBase64String(st);

				// Rijndael �̃T�[�r�X �v���o�C�_�𐶐����܂��B
                using (RijndaelManaged myRijndael = new RijndaelManaged())
                {
                    // ���o�͗p�̃X�g���[���𐶐����܂��B
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, myRijndael.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                        {
                            // �X�g���[���ɕ���������f�[�^���������݂܂��B
                            cs.Write(source, 0, source.Length);
                            cs.Close();
                            // ���������ꂽ�f�[�^�� byte �z��Ŏ擾���܂��B
                            byte[] destination = ms.ToArray();
                            ms.Close();
                            //	byte �z��𕶎���ɕϊ����ĕ\�����܂��B
                            ASCIIEncoding encoding = new ASCIIEncoding();
                            // �������F�؃L�[ 
                            decryptionTicket = encoding.GetString(destination);
                        }
                    }
                }
				// ���\�b�h����I�����L�^���܂��B
				PCSiteTraceSource.MethodSuccess();
				return decryptionTicket;
			}
			catch (Exception ex)
			{
				//	�A�v���P�[�V�����G���[���g���[�X���܂��B
				PCSiteTraceSource.AppError("SingleSignOn.DecryptString���Ăяo�����ۂ�Exception", st, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}

		/// <summary>
		///	�Í����L�[�A�����x�N�^�[�쐬
		/// </summary>
		/// <param name="key">�L�[</param>
		/// <param name="iv">�����x�N�^�[</param>
		/// <dl>
		///		<dt>����I���P�[�X</dt>
		///		<dd>�L�[�A�����x�N�^�[���Ԃ�܂��B</dd>
		///		<dt>�G���[�P�[�X</dt>
		///		<dd>�Í����L�[�A�����x�N�^�[�̎擾�Ɏ��s�����ꍇ�AAbort�����Ƃ��܂��B
		///		</dd>
		/// </dl>
		private static void GenerateKey(ref byte[] key, ref byte[] iv)
		{
			//	���\�b�h�J�n���L�^���܂��B
			PCSiteTraceSource.MethodStart();

			try
			{
				//	machine.confing����Í����L�[���擾���܂��B
                string key_temp = Config.Item["Toyota.Gbook.WebSite.SingleSignOn.TicketEncryptionKey"];
				
				//	��������o�C�g�^�ɕϊ�����B
                key = new byte[SingleSignOn.EncryptTicketKeyIndex];
                for (int i = 0, j = 0; j < SingleSignOn.EncryptTicketKeyIndex; i += 2, j++)
				{
					key[j] = Convert.ToByte(key_temp.Substring(i, 2), 16); 
				}
					
				//	machine.confing���珉���x�N�^�[���擾���܂��B
                string iv_temp = Config.Item["Toyota.Gbook.WebSite.SingleSignOn.TicketEncryptionIV"];
				
				//	��������o�C�g�^�ɕϊ�����B
                iv = new byte[SingleSignOn.EncryptTicketIVIndex];
                for (int i = 0, j = 0; j < SingleSignOn.EncryptTicketIVIndex; i += 2, j++)
				{
					iv[j] = Convert.ToByte(key_temp.Substring(i, 2), 16);
				}

				//	���\�b�h����I�����L�^���܂��B
				PCSiteTraceSource.MethodSuccess();
			}
			catch (Exception ex)
			{
				// �A�v���P�[�V�����G���[���g���[�X���܂��B
				PCSiteTraceSource.AppError("SingleSignOn.GenerateKey���Ăяo�����ۂ�Exception", key, iv, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}
	}
}
