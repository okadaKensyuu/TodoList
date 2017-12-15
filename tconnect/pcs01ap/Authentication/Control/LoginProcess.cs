using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Toyota.Gbook.WebSite.Common;
using Toyota.Gbook.WebSite.Common.Log;
using Toyota.Gbook.WebSite.Security.Business;
using Toyota.Gbook.WebSite.Security.Control;
using Toyota.Gbook.WebSite.Security.DataTransferObject;
using Toyota.Gbook.WebSite.Security.Exception;

namespace Toyota.Gbook.WebSite.Authentication.Control
{
    /// <summary>
    /// 認証結果のクラスです。
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// 認証結果を定義します。
        ///Succsess:認証OK
        ///Unauthorize：認証NG
        ///NoRelation：認証はOKだが、T-ConnectIdとの紐づけなし。
        /// </summary>
        public enum AuthResultStatus
        {
            Success,
            Unauthorize,
            NoRelation,
        }

        public AuthResultStatus Status { get; private set; }
        public OneIdTConnectAuthResult OneIdAuthResult {get; private set;}


        private AuthResult(AuthResultStatus status, OneIdTConnectAuthResult oneIdAuthResult)
        {
            this.Status = status;
            this.OneIdAuthResult = oneIdAuthResult;
        }

        public static AuthResult From(OneIdTConnectAuthResult oneIdAuthResult)
        {
            if(oneIdAuthResult.HasUsers())
            {
                return new AuthResult(AuthResultStatus.Success, oneIdAuthResult);
            }

            return new AuthResult(AuthResultStatus.NoRelation, oneIdAuthResult);
        }

        public static AuthResult Unauthorize() { return new AuthResult(AuthResultStatus.Unauthorize, null); }
        public static AuthResult NoRelation() { return new AuthResult(AuthResultStatus.NoRelation, null); }
    }

    /// <summary>
    /// ログイン処理を扱うクラスです。
    /// </summary>
    public class LoginProcess
    {
        public class AuthenticationDataSet
        {
            /// <summary>
            /// 認証結果データセット
            /// </summary>
            private ResultCDAuthenticationUserDataSet authDataSet;
            /// <summary>
            /// 認証結果データセット
            /// </summary>
            public ResultCDAuthenticationUserDataSet AuthDataSet
            {
                get { return this.authDataSet; }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public AuthenticationDataSet()
            {
            }


        }
        /// <summary>
        /// セッションキー定義値クラス
        /// </summary>
        public class SessionKey
        {
            /// <summary>
            /// 車両情報
            /// </summary>
            public const string VEHICLELIST = Constants.PCSiteNameSpace + ".VehicleList";
            /// <summary>
            /// ログインID登録要否
            /// </summary>
            public const string ISIDSAVED = Constants.PCSiteNameSpace + ".IdSaveChecked";
            /// <summary>
            /// 認証結果詳細情報
            /// </summary>
            public const string USERDATASET = Constants.PCSiteNameSpace + ".UserDataSet";
            /// <summary>
            /// サービス区分
            /// </summary>
            public const string SERVICE_DIVISION = Constants.PCSiteNameSpace + ".ServiceDivision";
            /// <summary>
            /// 車載機バージョン
            /// </summary>
            public const string GBML_VERSION = Constants.PCSiteNameSpace + ".GBML_VERSION";
            /// <summary>
            /// 接続区分
            /// </summary>
            public const string CONNECT_DIVISION = Constants.PCSiteNameSpace + ".ConnectDivision";
            /// <summary>
            /// ID
            /// </summary>
            public const string TCONNECT_ID = Constants.PCSiteNameSpace + ".TConnectId";
            /// <summary>
            /// パスワード
            /// </summary>
            public const string PASSWORD = Constants.PCSiteNameSpace + ".Password";
            /// <summary>
            /// 内部会員ID
            /// </summary>
            public const string INTERNAL_MEMBER_ID = Constants.PCSiteNameSpace + ".InternalMemberId";
            /// <summary>
            /// 会員属性
            /// </summary>
            public const string MEMBER_DIVISION = Constants.PCSiteNameSpace + ".MemberDivision";
            /// <summary>
            /// プレサイトユーザフラグ
            /// </summary>
            public const string IS_PRESITE_USER = Constants.PCSiteNameSpace + ".IsPreSiteUser";
            /// <summary>
            /// 解約済ユーザフラグ
            /// </summary>
            public const string IS_TERMINATED_USER = Constants.PCSiteNameSpace + ".IsTerminatedUser";
            /// <summary>
            /// ESPO用車載機モデル
            /// </summary>
            public const string ESPO_MODEL = "espo_model";
        }

        /// <summary>
        /// セッションキーリスト
        /// </summary>
        private static List<string> _SessionKeyList = new List<string>();

        #region // 定義値
        /// <summary>
        /// Securityにて設定されるDMC接続時の接続区分
        /// </summary>
        private const string SECURITY_DCM_CONNECT_VALUE = "2";

        /// <summary>
        /// Securityにて設定される携帯接続時の接続区分
        /// </summary>
        private const string SECURITY_MOBILE_PHONE_CONNECT_VALUE = "1";

        /// <summary>
        /// 処理結果コード G-BOOK未契約
        /// </summary>
        private const string GBOOK_UN_CONTRACT = "143";
        #endregion

        #region // プロパティ
        /// <summary>
        /// 認証I/Fクラス
        /// </summary>
        public AuthenticationDataSet SecurityAuth { get; private set; }

        /// <summary>
        /// 認証インタフェース 処理結果コード
        /// </summary>
        public string SecurityResultCode { get; private set; }

        /// <summary>
        /// T-Connect Navi判定
        /// </summary>
        public bool? IsTConnectNavi { get; private set; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LoginProcess()
        {
            PCSiteTraceSource.MethodStart();

            // メンバ初期化
            if (_SessionKeyList.Count == 0)
            {
                lock (_SessionKeyList)
                {
                    if (_SessionKeyList.Count == 0)
                    {
                        _SessionKeyList.AddRange(
                            typeof(SessionKey).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                            .Select(t => t.GetValue(null).ToString()));
                    }
                }
            }

            SecurityAuth = new AuthenticationDataSet();

            PCSiteTraceSource.MethodSuccess();
        }


        /// <summary>
        /// 認証処理を実施します。
        /// </summary>
        /// <param name="id">入力されたログインId</param>
        /// <param name="password">入力されたパスワード</param>
        /// <returns>認証結果</returns>
        public AuthResult Auth(string id, string password)
        {
            PCSiteTraceSource.MethodStart();
            var auth = new Toyota.Gbook.WebSite.Security.Control.Authentication();
            try
            {
                var apiResult = auth.GetOneIdAuthResult(id, password);
                return AuthResult.From(apiResult);

            }
            catch (UnauthorizedException)
            {
                PCSiteTraceSource.CheckPoint("認証APIがHTTPステータス401を返却");
                return AuthResult.Unauthorize();
            }
            catch (NoRelationException)
            {
                PCSiteTraceSource.CheckPoint("対象のIDに紐づいたT-ConnectIDなし");
                return AuthResult.NoRelation();
            }
        }

        /// <summary>
        /// テレマサービス情報APIの結果をDataSetにして返却します。
        /// </summary>
        /// <param name="id">入力されたログインId</param>
        /// <param name="result">認証結果</param>
        /// <returns>会員情報が設定されたDataSet</returns>
        public IList<Toyota.Gbook.WebSite.Security.Control.Authentication.Vehicle> GetVehicleList(OneIdTConnectAuthResult result)
        {
            var auth = new Toyota.Gbook.WebSite.Security.Control.Authentication();
            return auth.CreateVehicleList(result);
        }


        /// <summary>
        /// テレマサービス情報APIの結果をDataSetにして返却します。
        /// </summary>
        /// <param name="id">入力されたログインId</param>
        /// <param name="internalMemberIds">内部会員IDのリスト</param>
        /// <returns>会員情報が設定されたDataSet</returns>
        public ResultCDAuthenticationUserDataSet GetMemberInformationWithVehicle(string internalMemberId)
        {
            var auth = new Toyota.Gbook.WebSite.Security.Control.Authentication();
            return auth.CreateResultCDAuthenticationDataSet(internalMemberId);
        }


        /// <summary>
        /// 認可APIの結果をDataSetに格納にし、認証結果をboolで返却します。
        /// </summary>
        /// <param name="userDataSet">認証結果のデータセット</param>
        public bool AuthTConnectNavi(ResultCDAuthenticationUserDataSet userDataSet)
        {
            var auth = new Toyota.Gbook.WebSite.Security.Control.Authentication();
            bool isTConnectNavi;
            try
            {
                isTConnectNavi = auth.SetIsTConnectNaviFlag(userDataSet);

                // 認証インタフェース処理結果コード
                SecurityResultCode = userDataSet.ReturnInformation[0].ResultCD;

                // T-ConnectNavi判定結果
                if (!userDataSet.CarInformation[0].IsIsTConnectNaviNull())
                {
                    IsTConnectNavi = userDataSet.CarInformation[0].IsTConnectNavi;
                    isTConnectNavi = (bool)IsTConnectNavi;
                }

                if (!isTConnectNavi)
                {
                    // 143:G-BOOK未契約
                    isTConnectNavi = SecurityResultCode == GBOOK_UN_CONTRACT;
                }

                if (isTConnectNavi)
                {
                    // 認証ＯＫ
                    // 接続区分巻き替え
                    for (int i = 0; i < userDataSet.CarInformation.Count; i++)
                    {
                        string conn = userDataSet.CarInformation[i].ConnectDivision;
                        if (conn == SECURITY_DCM_CONNECT_VALUE)
                        {
                            userDataSet.CarInformation[i].ConnectDivision = Common.Constants.ConnectDCM;
                        }
                        else if (conn == SECURITY_MOBILE_PHONE_CONNECT_VALUE)
                        {
                            userDataSet.CarInformation[i].ConnectDivision = Common.Constants.ConnectMobilePhone;
                        }
                    }

                    // セッション値設定
                    // 1件目のデータを取得する（2件以上のデータが存在することは想定外）
                    var carInfo = userDataSet.CarInformation[0];
                    var memberInfo = userDataSet.MemberInformation[0];

                    HttpContext.Current.Session[SessionKey.USERDATASET] = userDataSet.Copy();
                    HttpContext.Current.Session[SessionKey.SERVICE_DIVISION] = carInfo.ServiceDivision;
                    HttpContext.Current.Session[SessionKey.GBML_VERSION] = carInfo.GBML_VERSION;
                    HttpContext.Current.Session[SessionKey.CONNECT_DIVISION] = carInfo.ConnectDivision;
                    HttpContext.Current.Session[SessionKey.TCONNECT_ID] = memberInfo.TConnectId;
                    HttpContext.Current.Session[SessionKey.PASSWORD] = memberInfo.Password;
                    HttpContext.Current.Session[SessionKey.INTERNAL_MEMBER_ID] = memberInfo.InternalMemberId;
                    HttpContext.Current.Session[SessionKey.MEMBER_DIVISION] = memberInfo.MemberDivision;
                    HttpContext.Current.Session[SessionKey.IS_PRESITE_USER] = userDataSet.MemberInformation[0].PreSiteMemberFlag == "1"; // "1"=true
                    // ESPoサイトへ渡すSession情報を格納
                    HttpContext.Current.Session[SessionKey.ESPO_MODEL] = Constants.MODEL.MODEL_13;
                    HttpContext.Current.Session[SessionKey.IS_TERMINATED_USER] = SecurityResultCode == GBOOK_UN_CONTRACT;
                }
                else
                {
                    // 認証失敗
                    _SessionKeyList.ForEach(
                        key =>
                        {
                            if (HttpContext.Current.Session[key] != null)
                                HttpContext.Current.Session.Remove(key);
                        });
                }


            }
            catch (ApplicationException)
            {
                _SessionKeyList.ForEach(
                    key =>
                    {
                        if (HttpContext.Current.Session[key] != null)
                            HttpContext.Current.Session.Remove(key);
                    });
                // そのままthrow
                throw;
            }
            catch (Exception ex)
            {
                PCSiteTraceSource.AppError(ex.Message);
                _SessionKeyList.ForEach(
                    key =>
                    {
                        if (HttpContext.Current.Session[key] != null)
                            HttpContext.Current.Session.Remove(key);
                    });
                throw new ApplicationException(ex.Message, ex);
            }

            PCSiteTraceSource.MethodSuccess();

            return isTConnectNavi;
        }
    }
}
