using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Services
{
    public class DokiNetServiceMock : IDokiNetService
    {
        private readonly List<DokiNetMember> _members = new List<DokiNetMember>()
        {
            new DokiNetMember() { MemberRightId = 100, Prefix = "", FirstName = "László", LastName = "Nagy", UserName = "laszlonagy", StampNumber = "00001", Emails = new[] { "laszlonagy@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 200, Prefix = "dr", FirstName = "Imre", LastName = "Horváth", UserName = "imrehorvath", StampNumber = "00020", Emails = new[] { "imrehorvath@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 300, Prefix = "prof", FirstName = "Csilla", LastName = "Balogh", UserName = "csillabalogh", StampNumber = "00301", Emails = new[] { "csillabalogh@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 400, Prefix = "prof dr", FirstName = "Ferenc", LastName = "Koczka", UserName = "ferikocz", StampNumber = "04001", Emails = new[] { "ferikocz@nodomain.wq", "ferike.szemelyes@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 500, Prefix = "", FirstName = "Gábor", LastName = "Kovács", UserName = "gkovi", StampNumber = "50001", Emails = new[] { "gkovi@nodomain.wq", "gabor.kovacs@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 610, Prefix = "", FirstName = "Zsolt", LastName = "Előd", UserName = "zselod1568", StampNumber = "00006", Emails = new[] { "zsolt.el@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 650, FirstName = "István", LastName = "Kiss", UserName = "istvan.kiss", StampNumber = "00777", Emails = new[] { "kiss.pistike@nodomain.wq", "kpista@nomail.io" }, HasMemberShip = true, IsDueOk = true, DiabetLicenceNumber = "abcd123" },
            new DokiNetMember() { MemberRightId = 129, FirstName = "Lilla", LastName = "Lakatos", UserName = "laklil", StampNumber = null, Emails = new[] { "laklil@organimedcore.qq", "laklil@nodomain.wq", "lak-lil@my-home.io" }, HasMemberShip = true, IsDueOk = true },

            // Login is not allowed for this user (not member)
            new DokiNetMember() { MemberRightId = 5, FirstName = "Hugó", LastName = "Hamis", UserName = "hamhug", StampNumber = null, Emails = new[] { "hamishugo@nodomain.wq" } },

            new DokiNetMember() { MemberRightId = 1444, Prefix="Mr.", FirstName = "András", LastName = "Alsó", UserName = "alsobandy", StampNumber = "15684", Emails = new[] { "nagy.bandy@nodomain.wq" }, HasMemberShip = true, IsDueOk = false },
            new DokiNetMember() { MemberRightId = 1555, Prefix="Miss", FirstName = "Júlia", LastName = "Boczi", UserName = "boczijuli", StampNumber = "82308", Emails = new[] { "boczi@juli.hu", "boczi@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 7079, FirstName = "Hugó", LastName = "Hollós", UserName = "hhugo12", StampNumber = "50577", Emails = new[] { "hhugi@holl.hu", "hugesz@nodomain.wq" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 395851, FirstName = "Szilárd", LastName = "", UserName = "cssz", StampNumber = "15190", Emails = new[] { "szilard.csere@intellimed.eu" }, HasMemberShip = true, IsDueOk = true },
            new DokiNetMember() { MemberRightId = 9478, FirstName = "Ádám", LastName = "Virgonc", UserName = "vadingc", StampNumber = "77773", Emails = new[] { "vadingc@nodomain.wq" }, HasMemberShip = true, IsDueOk = true, DiabetLicenceNumber = "adamdiab2014" }
        };


        public Task<T> GetDokiNetMemberById<T>(int memberRightId) where T : DokiNetMember
            => Task.FromResult(_members.FirstOrDefault(x => x.MemberRightId == memberRightId) as T);

        public Task<T> GetDokiNetMemberByLoginAsync<T>(string username, string password) where T : DokiNetMember
            => Task.FromResult(_members.FirstOrDefault(x => x.UserName == username) as T);

        public Task<T> GetDokiNetMemberByNonce<T>(string nonce) where T : DokiNetMember
        {
            var nonceTable = new Dictionary<string, DokiNetMember>()
            {
                { "nonce1", _members[0] },
                { "nonce2", _members[1] },
                { "nonce3", _members[2] },
                { "nonce4", _members[3] },
                { "nonce5", _members[4] },
                { "nonce6", _members[5] },
                { "nonce7", _members[6] },
                { "nonce8", _members[7] },
                { "nonce10", _members[9] }
            };

            if (!nonceTable.ContainsKey(nonce))
            {
                throw new MemberNotFoundException();
            }

            return Task.FromResult(nonceTable[nonce] as T);

        }

        public Task<IEnumerable<T>> GetDokiNetMembersByIds<T>(IEnumerable<int> memberRightIds) where T : DokiNetMember
        {
            memberRightIds.ThrowIfNull();

            return Task.FromResult(
                _members.Where(m => memberRightIds.Contains(m.MemberRightId)).Select(m => m as T));
        }

        public Task SaveMemberDataAnsyc(int memberId, IEnumerable<MemberData> values)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> SearchDokiNetMemberByName<T>(string name) where T : DokiNetMember
        {
            return Task.FromResult(_members.Where(member =>
            {
                return member.FullName.ToLowerInvariant().Contains(name.ToLowerInvariant());
            }) as IEnumerable<T>);
        }

        public Task<MembershipWatchResponse> WatchMembershipAsync(DateTime lastCheckDateUtc)
            => Task.FromResult(new MembershipWatchResponse()
            {
                MemberRightIds = new[] { 100, 200, 300, 400 }
            });
    }
}
