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
	/// SingleSignOnクラスです。
	/// </summary>
	public class SingleSignOn
	{
        #region // シングルサインオンインターフェースで利用する定数
        /// <summary>
        ///	暗号化キーの配列のインデックス
        /// </summary>
        public const int EncryptTicketKeyIndex = 32;

        /// <summary>
        ///	暗号化初期ベクターの配列のインデックス
        /// </summary>
        public const int EncryptTicketIVIndex = 16;
        #endregion
        
		/// <summary>
		///	コンストラクタ
		/// </summary>
		public SingleSignOn()
		{
		}

		/// <summary>
        /// URL生成
		/// </summary>
        /// <remarks>
        /// <para>この関数は、validationUrl + "?targeturl=" + targetUrl + "&amp;ticket=" + key の形式のURL文字列を生成します。</para>
        /// <para>keyは暗号化され、QueryStringとして付与されます。</para>
        /// </remarks>
        /// <param name="validationUrl">検証先URL</param>
        /// <param name="targetUrl">遷移先URL</param>
		/// <param name="key">キー</param>
		/// <returns>認証キーを含むリダイレクト先URL</returns>
		public string GenerateURL(string validationUrl, string targetUrl, string key)
		{
			//	メソッド開始を記録します。
			PCSiteTraceSource.MethodStart(validationUrl, targetUrl, key);

            // 検証先ページURLのチェック（null、空白チェック）
            if (string.IsNullOrEmpty(validationUrl))
            {
                throw new ArgumentNullException("validationUrl");
            }
			// 遷移先ページ識別子のチェック（null、空白チェック）
            if (string.IsNullOrEmpty(targetUrl))
			{
                throw new ArgumentNullException("targetUrl");
			}
			// キー値のチェック（null、空白チェック）
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}

			try
			{
				string returnURL = "";
			
				PCSiteTraceSource.CheckPoint("GenerateURLメソッドチェック(ターゲットURL)：" + targetUrl);

				//	SingleSignOnTicketクラスのGentrateメソッド呼び出し
				string encriptTicket = SingleSignOnAuthKey.Generate(key); 

				PCSiteTraceSource.CheckPoint("GenerateURLメソッドチェック(暗号化チケット)：" + encriptTicket);

				// 認証キーを含むリダイレクト先URL作成
				StringBuilder sb = new StringBuilder();
				sb.Append(validationUrl);
				sb.Append("?targeturl=");

				// targetの値をURLエンコードします。
                sb.Append(System.Web.HttpUtility.UrlEncode(targetUrl));
				sb.Append("&ticket=");

				//	 ase64でエンコードした時に「+、/」が入るためURLエンコードします。
				sb.Append(System.Web.HttpUtility.UrlEncode(encriptTicket));
				returnURL = sb.ToString();
				
				PCSiteTraceSource.CheckPoint("GenerateURLメソッドチェック(ReturnUrl)：" + returnURL);

				//	メソッド正常終了を記録します。
				PCSiteTraceSource.MethodSuccess();
				return returnURL;
			}
			catch (Exception ex)
			{
				//	アプリケーションエラーをトレースします。
				PCSiteTraceSource.AppError("SingleSignOn.GenerateURLを呼び出した際のException", validationUrl, targetUrl, key, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}

		/// <summary>
		///	暗号化<br/>
		///	Rijndael アルゴリズムを使用<br/>
		/// </summary>
		/// <param name="st">認証キー</param>
		/// <returns>暗号化された認証キー</returns>
		internal static string EncryptString(string st)
		{
			//	チケットのチェック（null、空白チェック）
			if (st == null || st == "")
			{
				throw new ArgumentNullException("Key");
			}
			
			//	メソッド開始を記録します。
			PCSiteTraceSource.MethodStart(st);

			try
			{
				// 暗号化された認証キー
				string encryptionTicket;

				byte[] key = null;
				byte[] iv = null;

				// 暗号化キーを作成します。
				GenerateKey(ref key, ref iv);
		 
				// 入力文字列を byte 配列に変換します。
				ASCIIEncoding encoding = new ASCIIEncoding();
				byte[] source = encoding.GetBytes(st);

				// Rijndael のサービス プロバイダを生成します。
                using (RijndaelManaged myRijndael = new RijndaelManaged())
                {
                    // 入出力用のストリームを生成します。
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, myRijndael.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                        {
                            // ストリームに暗号化するデータを書き込みます。
                            cs.Write(source, 0, source.Length);
                            cs.Close();

                            // 暗号化されたデータを byte 配列で取得します。
                            byte[] destination = ms.ToArray();
                            ms.Close();

                            // byte 配列を文字列に変換して表示します。
                            encryptionTicket = Convert.ToBase64String(destination);
                        }
                    }
                }
				// メソッド正常終了を記録します。
				PCSiteTraceSource.MethodSuccess();
				return encryptionTicket;
			}
			catch (Exception ex)
			{
				// アプリケーションエラーをトレースします。
				PCSiteTraceSource.AppError("SingleSignOn.EncryptStringを呼び出した際のException", st, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}
		
		/// <summary>
		///	復号化<br/>
		///	Rijndael アルゴリズムを使用<br/>
		/// </summary>
		/// <param name="st">暗号化された認証キー</param>
		/// <returns>復号化された認証キー</returns>
		internal static string DecryptString(string st)
		{
			// パラメータのチェック（null、空白チェック）
			if (st == null || st == "")
			{
				throw new ArgumentNullException("Ticket");
			}

			// メソッド開始を記録します。
			PCSiteTraceSource.MethodStart(st);

			try
			{
				// 復号化認証キー
				string decryptionTicket;

				// パラメータのnull、空白チェック
				if(st == null || st == "")
				{
					throw new ArgumentNullException("st");
				}

				byte[] key = null;
				byte[] iv = null;
				
				// 暗号化キーを作成します。
				GenerateKey(ref key, ref iv);

				// 文字列をBese64でデコードしてbyte 配列に変換します。
				byte[] source = Convert.FromBase64String(st);

				// Rijndael のサービス プロバイダを生成します。
                using (RijndaelManaged myRijndael = new RijndaelManaged())
                {
                    // 入出力用のストリームを生成します。
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, myRijndael.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                        {
                            // ストリームに復号化するデータを書き込みます。
                            cs.Write(source, 0, source.Length);
                            cs.Close();
                            // 復号化されたデータを byte 配列で取得します。
                            byte[] destination = ms.ToArray();
                            ms.Close();
                            //	byte 配列を文字列に変換して表示します。
                            ASCIIEncoding encoding = new ASCIIEncoding();
                            // 復号化認証キー 
                            decryptionTicket = encoding.GetString(destination);
                        }
                    }
                }
				// メソッド正常終了を記録します。
				PCSiteTraceSource.MethodSuccess();
				return decryptionTicket;
			}
			catch (Exception ex)
			{
				//	アプリケーションエラーをトレースします。
				PCSiteTraceSource.AppError("SingleSignOn.DecryptStringを呼び出した際のException", st, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}

		/// <summary>
		///	暗号化キー、初期ベクター作成
		/// </summary>
		/// <param name="key">キー</param>
		/// <param name="iv">初期ベクター</param>
		/// <dl>
		///		<dt>正常終了ケース</dt>
		///		<dd>キー、初期ベクターが返ります。</dd>
		///		<dt>エラーケース</dt>
		///		<dd>暗号化キー、初期ベクターの取得に失敗した場合、Abort扱いとします。
		///		</dd>
		/// </dl>
		private static void GenerateKey(ref byte[] key, ref byte[] iv)
		{
			//	メソッド開始を記録します。
			PCSiteTraceSource.MethodStart();

			try
			{
				//	machine.confingから暗号化キーを取得します。
                string key_temp = Config.Item["Toyota.Gbook.WebSite.SingleSignOn.TicketEncryptionKey"];
				
				//	文字列をバイト型に変換する。
                key = new byte[SingleSignOn.EncryptTicketKeyIndex];
                for (int i = 0, j = 0; j < SingleSignOn.EncryptTicketKeyIndex; i += 2, j++)
				{
					key[j] = Convert.ToByte(key_temp.Substring(i, 2), 16); 
				}
					
				//	machine.confingから初期ベクターを取得します。
                string iv_temp = Config.Item["Toyota.Gbook.WebSite.SingleSignOn.TicketEncryptionIV"];
				
				//	文字列をバイト型に変換する。
                iv = new byte[SingleSignOn.EncryptTicketIVIndex];
                for (int i = 0, j = 0; j < SingleSignOn.EncryptTicketIVIndex; i += 2, j++)
				{
					iv[j] = Convert.ToByte(key_temp.Substring(i, 2), 16);
				}

				//	メソッド正常終了を記録します。
				PCSiteTraceSource.MethodSuccess();
			}
			catch (Exception ex)
			{
				// アプリケーションエラーをトレースします。
				PCSiteTraceSource.AppError("SingleSignOn.GenerateKeyを呼び出した際のException", key, iv, ex);
				throw new ApplicationException(ex.Message, ex);
			}
		}
	}
}
