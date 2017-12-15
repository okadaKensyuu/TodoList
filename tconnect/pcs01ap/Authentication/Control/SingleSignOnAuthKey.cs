using Gbook.Base.Configuration;
using System;
using System.Configuration;
using System.Text;
using Toyota.Gbook.WebSite.Common.Log;

namespace Toyota.Gbook.WebSite.Authentication.Control
{
	/// <summary>
    /// SingleSignOnAuthKeyクラスです。
	/// </summary>
    public class SingleSignOnAuthKey
	{
		/// <summary>
		///	コンストラクタ
		/// </summary>
		public SingleSignOnAuthKey()
		{
		}

		/// <summary>
		///	認証キー検証
		/// </summary>
		/// <param name="authenticationKey">認証キー</param>
		/// <returns>キー値</returns>
		public string Validate(string authenticationKey)
		{
			//	認証キーのチェック（null、空白チェック）
			if (authenticationKey == null || authenticationKey == "")
			{
                throw new ArgumentNullException("authenticationKey");
			}

			//	メソッド開始を記録します。
            PCSiteTraceSource.MethodStart(authenticationKey);

			try
			{
				string key;

				//	復号化メソッド呼び出し後、キー情報の取り出し
				string[] condition = SingleSignOn.DecryptString(authenticationKey).Split(",".ToCharArray());
			
				PCSiteTraceSource.CheckPoint("Validateメソッドチェック(キー)：" + condition[0]);
				PCSiteTraceSource.CheckPoint("Validateメソッドチェック(チケット作成時間)：" + condition[1]);
				PCSiteTraceSource.CheckPoint("Validateメソッドチェック(チケット有効期限)：" + condition[2]);

				//	認証キー作成時間を取得
				DateTime dt = Convert.ToDateTime(condition[1]);
			
				//	有効期限チェック：システム日付＜認証キー作成時+ 有効期限(秒)
				if (DateTime.Now < dt.AddSeconds(Convert.ToDouble(condition[2])))
				{
					key = condition[0];
				}
				else
				{
					key = null;
				}

				PCSiteTraceSource.CheckPoint("Validateメソッドチェック(return値)：" + key);

				//	メソッド正常終了を記録します。
				PCSiteTraceSource.MethodSuccess();
				return key;
			}
			catch (Exception ex)
			{
				//	アプリケーションエラーをトレースします。
                PCSiteTraceSource.AppError("SingleSignOnTicket.Validateを呼び出した際のException", authenticationKey, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}

		/// <summary>
		///	認証キー生成
		/// </summary>
		/// <param name="key">キー値</param>
		/// <returns>認証キー</returns>
        internal static string Generate(string key)
		{
			//	キー値のチェック（null、空白チェック）
			if (key == null || key == "")
			{
                throw new ArgumentNullException("key");
			}

			//	メソッド開始を記録します。
			PCSiteTraceSource.MethodStart(key);

			try
			{
				//	machine.confingから有効期限（秒）を取得します。
                string ExpirationTime = Config.Item["Toyota.Gbook.WebSite.SingleSignOn.ExpirationTime"];
				
				StringBuilder sb = new StringBuilder(); 
				sb.Append(key);
				sb.Append(",");
				sb.Append(Toyota.Gbook.WebSite.Common.JapaneseDateTime.Now.ToString());
				sb.Append(",");
				sb.Append(ExpirationTime);
			
				//	メソッド正常終了を記録します。
				PCSiteTraceSource.MethodSuccess();

				//	暗号化メソッド呼び出し
				return SingleSignOn.EncryptString(sb.ToString()); 
			}
			catch (Exception ex)
			{
				//	アプリケーションエラーをトレースします。
				PCSiteTraceSource.AppError("SingleSignOnTicket.Generateを呼び出した際のException", key, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}
	}
}
