using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toyota.Gbook.WebSite.Security.Business;
using Toyota.Gbook.WebSite.Security.Exception;
using Toyota.Gbook.WebSite.DataAccess.Common.Control;
using Toyota.Gbook.WebSite.Security.Exception;
using Toyota.Gbook.WebSite.DataAccess.Member.Entity;
using Toyota.Gbook.WebSite.Common.Log;
using Toyota.Gbook.WebSite.Security;


namespace Toyota.Gbook.WebSite.Authentication.Control
{
    public class Reminder
    {
        public List<MemberInfo> CheckPersonalMemberExist(string loginId, string birthday, string email)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            var memberInfoList = reminderAuth.CheckPersonalMemberExist(loginId, birthday, email);
            foreach (var m in memberInfoList)
            {
                IsPersonalMember(m.InternalMemberId);
            }

            return memberInfoList;
        }

        /// <summary>
        /// メールアドレス登録用の法人会員存在チェックを実施します。
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="email"></param>
        /// <param name="vin"></param>
        /// <returns></returns>
        public MemberInfo CheckCompanyMemberExist(string loginId, string email, string vin)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            //HACK: 法人の場合はWebApiDriverの中で会員区分のチェックをしているが、個人に合わせてここでメソッドを作って検証した方がいいかもしれない。
            return reminderAuth.CheckCompanyMemberExist(loginId, email, vin);
        }

        /// <summary>
        /// パスワード再設定用の法人会員存在チェックを実施します。
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="vin"></param>
        /// <returns></returns>
        public string CheckCompanyMemberExist(string loginId, string vin)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.CheckCompanyMemberExist(loginId, vin);
        }

        public void IsPersonalMember(string internalMemberId)
        {
            Toyota.Gbook.WebSite.DataAccess.Member.Entity.MemberInformationEntity mEntity =
            Toyota.Gbook.WebSite.DataAccess.Member.Control.Member.GetMemberInfo(internalMemberId);
            var memberDivisionPersonal = "1";
            if (string.IsNullOrEmpty(mEntity.MemberDivision))
            {
                PCSiteTraceSource.AppError("会員区分が取得できませんでした。内部会員Id：", internalMemberId);
                throw new UserNotFoundException(internalMemberId, "個人", "会員区分が取得できませんでした。");
            }

            if (mEntity.MemberDivision == "0")
            {
                throw new UnexpectedMemberDivisionException(internalMemberId, "個人", "会員区分「0：ID、Passのみ」が返却されました。");
            }
            else if (mEntity.MemberDivision != memberDivisionPersonal)
            {
                var memberDivision = string.Format("入力された会員区分：{0}, APIのレスポンスから取得した会員区分: {1}", "個人", "");
                throw new UserNotFoundException(internalMemberId, "個人", "指定されたユーザーが存在しません。" + memberDivision);
            }
        }

        public bool IsAccountLocked(string tconnectId)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.IsAccountLocked(tconnectId);
        }

        public string PasswordResetMailSendForPerson(string memberId, string email, string birthday)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.PasswordResetMailSendForPerson(memberId, email, birthday);
        }

        public string PasswordResetMailSendForCompany(string loginId, string email, string vin)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.PasswordResetMailSendForCompany(loginId, email, vin);
        }

        public string IdRemindMailSendForPerson(string email, string birthday)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.IdRemindMailSendForPerson(email, birthday);
        }

        public string IdRemindMailSendForCompany(string email, string vin)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.IdRemindMailSendForCompany(email, vin);
        }

        public bool IsToyotaTConnectMember(string internalMemberId)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.IsToyotaTConnectMember(internalMemberId);
        }

        public string InquiryRegist(string id, string name, string birthday, string zip, string address1, string address2, string telephone, string mail, string c_zip, string c_address1, string c_address2, string c_telephone, string today, string now, string PCSITE)
        {
            var reminderAuth = new Toyota.Gbook.WebSite.Security.Business.ReminderAuth();
            return reminderAuth.InquiryRegist(id, name, birthday, zip
                , address1, address2, telephone, mail, c_zip, c_address1, c_address2, c_telephone
                , today, now, PCSITE);
        }
    }
}
