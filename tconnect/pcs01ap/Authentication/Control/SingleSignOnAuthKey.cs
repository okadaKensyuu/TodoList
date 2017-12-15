using Gbook.Base.Configuration;
using System;
using System.Configuration;
using System.Text;
using Toyota.Gbook.WebSite.Common.Log;

namespace Toyota.Gbook.WebSite.Authentication.Control
{
	/// <summary>
    /// SingleSignOnAuthKey�N���X�ł��B
	/// </summary>
    public class SingleSignOnAuthKey
	{
		/// <summary>
		///	�R���X�g���N�^
		/// </summary>
		public SingleSignOnAuthKey()
		{
		}

		/// <summary>
		///	�F�؃L�[����
		/// </summary>
		/// <param name="authenticationKey">�F�؃L�[</param>
		/// <returns>�L�[�l</returns>
		public string Validate(string authenticationKey)
		{
			//	�F�؃L�[�̃`�F�b�N�inull�A�󔒃`�F�b�N�j
			if (authenticationKey == null || authenticationKey == "")
			{
                throw new ArgumentNullException("authenticationKey");
			}

			//	���\�b�h�J�n���L�^���܂��B
            PCSiteTraceSource.MethodStart(authenticationKey);

			try
			{
				string key;

				//	���������\�b�h�Ăяo����A�L�[���̎��o��
				string[] condition = SingleSignOn.DecryptString(authenticationKey).Split(",".ToCharArray());
			
				PCSiteTraceSource.CheckPoint("Validate���\�b�h�`�F�b�N(�L�[)�F" + condition[0]);
				PCSiteTraceSource.CheckPoint("Validate���\�b�h�`�F�b�N(�`�P�b�g�쐬����)�F" + condition[1]);
				PCSiteTraceSource.CheckPoint("Validate���\�b�h�`�F�b�N(�`�P�b�g�L������)�F" + condition[2]);

				//	�F�؃L�[�쐬���Ԃ��擾
				DateTime dt = Convert.ToDateTime(condition[1]);
			
				//	�L�������`�F�b�N�F�V�X�e�����t���F�؃L�[�쐬��+ �L������(�b)
				if (DateTime.Now < dt.AddSeconds(Convert.ToDouble(condition[2])))
				{
					key = condition[0];
				}
				else
				{
					key = null;
				}

				PCSiteTraceSource.CheckPoint("Validate���\�b�h�`�F�b�N(return�l)�F" + key);

				//	���\�b�h����I�����L�^���܂��B
				PCSiteTraceSource.MethodSuccess();
				return key;
			}
			catch (Exception ex)
			{
				//	�A�v���P�[�V�����G���[���g���[�X���܂��B
                PCSiteTraceSource.AppError("SingleSignOnTicket.Validate���Ăяo�����ۂ�Exception", authenticationKey, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}

		/// <summary>
		///	�F�؃L�[����
		/// </summary>
		/// <param name="key">�L�[�l</param>
		/// <returns>�F�؃L�[</returns>
        internal static string Generate(string key)
		{
			//	�L�[�l�̃`�F�b�N�inull�A�󔒃`�F�b�N�j
			if (key == null || key == "")
			{
                throw new ArgumentNullException("key");
			}

			//	���\�b�h�J�n���L�^���܂��B
			PCSiteTraceSource.MethodStart(key);

			try
			{
				//	machine.confing����L�������i�b�j���擾���܂��B
                string ExpirationTime = Config.Item["Toyota.Gbook.WebSite.SingleSignOn.ExpirationTime"];
				
				StringBuilder sb = new StringBuilder(); 
				sb.Append(key);
				sb.Append(",");
				sb.Append(Toyota.Gbook.WebSite.Common.JapaneseDateTime.Now.ToString());
				sb.Append(",");
				sb.Append(ExpirationTime);
			
				//	���\�b�h����I�����L�^���܂��B
				PCSiteTraceSource.MethodSuccess();

				//	�Í������\�b�h�Ăяo��
				return SingleSignOn.EncryptString(sb.ToString()); 
			}
			catch (Exception ex)
			{
				//	�A�v���P�[�V�����G���[���g���[�X���܂��B
				PCSiteTraceSource.AppError("SingleSignOnTicket.Generate���Ăяo�����ۂ�Exception", key, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}
	}
}
