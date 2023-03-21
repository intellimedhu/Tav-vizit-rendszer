using IntelliMed.DokiNetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Services
{
    public interface IDokiNetService
    {
        Task<IEnumerable<T>> SearchDokiNetMemberByName<T>(string name) where T : DokiNetMember;

        Task<T> GetDokiNetMemberByNonce<T>(string nonce) where T : DokiNetMember;

        Task<T> GetDokiNetMemberById<T>(int memberRightId) where T : DokiNetMember;

        Task<IEnumerable<T>> GetDokiNetMembersByIds<T>(IEnumerable<int> memberRightIds) where T : DokiNetMember;

        Task<T> GetDokiNetMemberByLoginAsync<T>(string username, string password) where T : DokiNetMember;

        Task SaveMemberDataAnsyc(int memberId, IEnumerable<MemberData> values);

        Task<MembershipWatchResponse> WatchMembershipAsync(DateTime lastCheckDateUtc);
    }
}
