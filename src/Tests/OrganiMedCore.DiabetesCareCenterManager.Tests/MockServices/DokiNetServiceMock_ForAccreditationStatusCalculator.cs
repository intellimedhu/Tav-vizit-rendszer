using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices
{
    public class DokiNetServiceMock_ForAccreditationStatusCalculator : IDokiNetService
    {
        private readonly IEnumerable<DokiNetMember> _expectedResults;


        public DokiNetServiceMock_ForAccreditationStatusCalculator(IEnumerable<DokiNetMember> expectedResults)
        {
            _expectedResults = expectedResults;
        }


        public Task<IEnumerable<T>> GetDokiNetMembersByIds<T>(IEnumerable<int> memberRightIds) where T : DokiNetMember
            => Task.FromResult(_expectedResults as IEnumerable<T>);

        [ExcludeFromCodeCoverage]
        public Task<T> GetDokiNetMemberById<T>(int memberRightId) where T : DokiNetMember
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<T> GetDokiNetMemberByLoginAsync<T>(string username, string password) where T : DokiNetMember
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<T> GetDokiNetMemberByNonce<T>(string nonce) where T : DokiNetMember
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task SaveMemberDataAnsyc(int memberId, IEnumerable<MemberData> values)
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<T>> SearchDokiNetMemberByName<T>(string name) where T : DokiNetMember
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<MembershipWatchResponse> WatchMembershipAsync(DateTime lastCheckDateUtc)
            => throw new NotImplementedException();
    }
}
