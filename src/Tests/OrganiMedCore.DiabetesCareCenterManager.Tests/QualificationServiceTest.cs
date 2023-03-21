using IntelliMed.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class QualificationServiceTest
    {
        [Fact]
        public async Task UpdateQualification_AddNew_ShouldBeAdded()
        {
            var service = new QualificationService(new SiteServiceMock());

            var name = "Test qualification";
            await service.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = name
            });

            var settings = await service.GetQualificationSettingsAsync();

            Assert.Single(settings.Qualifications);
            Assert.Equal(settings.Qualifications.First().Name, name);
            Assert.NotEqual(default(Guid), settings.Qualifications.First().Id);
        }

        [Fact]
        public async Task UpdateQualification_Add2New_ShouldThrownArgumentException()
        {
            var service = new QualificationService(new SiteServiceMock());

            var name = "Test qualification";
            await service.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = name
            });

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = name
            }));
        }

        [Fact]
        public async Task UpdateQualification_Update_ShouldBeUpdated()
        {
            var service = new QualificationService(new SiteServiceMock());

            var name = "Test qualification";
            await service.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = name
            });

            var existingQualification = (await service.GetQualificationSettingsAsync()).Qualifications.First();

            var viewModel = new QualificationViewModel();
            viewModel.UpdateViewModel(existingQualification);

            var updatedName = "Test qualification updated";
            viewModel.Name = updatedName;

            await service.UpdateQualificationAsync(viewModel);

            var settings = await service.GetQualificationSettingsAsync();

            Assert.Equal(updatedName, settings.Qualifications.First().Name);
            Assert.Single(settings.Qualifications);
        }

        [Fact]
        public async Task UpdateQualification_Update_ShouldThrownArgumentException()
        {
            var service = new QualificationService(new SiteServiceMock());

            var name1 = "Test qualification";
            await service.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = name1
            });

            var name2 = "Test qualification 2";
            await service.UpdateQualificationAsync(new QualificationViewModel()
            {
                Name = name2
            });

            var settings = await service.GetQualificationSettingsAsync();

            var q1 = settings.Qualifications.First(x => x.Name == name1);

            var viewModel = new QualificationViewModel();
            viewModel.UpdateViewModel(q1);

            viewModel.Name = name2;

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateQualificationAsync(viewModel));
        }

        [Fact]
        public async Task UpdateQualification_Update_ShouldThrownNotFoundException()
        {
            var service = new QualificationService(new SiteServiceMock());

            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateQualificationAsync(new QualificationViewModel()
            {
                Id = Guid.Empty,
                Name = "Ex"
            }));
        }

        [Fact]
        public async Task UpdateQualificationsPerOccupationsAsync_ShouldAddedAllSelected()
        {
            var service = new QualificationService(new SiteServiceMock());

            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });
            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });
            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

            var qualifications = (await service.GetQualificationSettingsAsync()).Qualifications;

            var vm1 = new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Doctor,
                QualificationId = qualifications.Skip(0).First().Id // A
            };

            var vm2 = new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Doctor,
                QualificationId = qualifications.Skip(1).First().Id // B
            };

            var vm3 = new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Physiotherapist,
                QualificationId = qualifications.Skip(2).First().Id // C
            };

            var vm4 = new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Nurse,
                QualificationId = qualifications.Skip(3).First().Id // D
            };

            var vm5 = new QualificationPerOccupationViewModel()
            {
                Occupation = Occupation.Other,
                QualificationId = qualifications.Skip(2).First().Id // C
            };

            var vm6 = new QualificationPerOccupationViewModel()
            {
                Occupation = Occupation.DiabetesNurseEducator,
                QualificationId = qualifications.Skip(2).First().Id // C
            };

            await service.UpdateQualificationsPerOccupationsAsync(
                new CenterProfileQualificationSettingsViewModel()
                {
                    QualificationsPerOccupations = new List<QualificationPerOccupationViewModel>()
                    {
                        vm1,
                        vm2,
                        vm3,
                        vm4,
                        vm5,
                        vm6
                    }
                });

            var settings = await service.GetQualificationSettingsAsync();

            Assert.True(settings.QualificationsPerOccupations.Count == 3);

            Assert.True(settings.QualificationsPerOccupations.ContainsKey(Occupation.Doctor));
            Assert.True(settings.QualificationsPerOccupations.ContainsKey(Occupation.Physiotherapist));
            Assert.True(settings.QualificationsPerOccupations.ContainsKey(Occupation.Nurse));

            Assert.False(settings.QualificationsPerOccupations.ContainsKey(Occupation.Other));
            Assert.False(settings.QualificationsPerOccupations.ContainsKey(Occupation.DiabetesNurseEducator));
        }

        [Fact]
        public async Task DeleteQualificationAsync_ShouldBeDeleted()
        {
            var service = new QualificationService(new SiteServiceMock());
            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "B" });
            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "C" });
            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "D" });

            var qualifications = (await service.GetQualificationSettingsAsync()).Qualifications;

            var viewModel = new CenterProfileQualificationSettingsViewModel();
            viewModel.QualificationsPerOccupations.Add(new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Doctor,
                QualificationId = qualifications.Skip(0).First().Id // A
            });

            viewModel.QualificationsPerOccupations.Add(new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Doctor,
                QualificationId = qualifications.Skip(1).First().Id // B
            });

            viewModel.QualificationsPerOccupations.Add(new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Doctor,
                QualificationId = qualifications.Skip(2).First().Id // C
            });

            viewModel.QualificationsPerOccupations.Add(new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Dietician,
                QualificationId = qualifications.Skip(2).First().Id // C
            });

            viewModel.QualificationsPerOccupations.Add(new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.Dietician,
                QualificationId = qualifications.Skip(3).First().Id // D
            });

            viewModel.QualificationsPerOccupations.Add(new QualificationPerOccupationViewModel()
            {
                IsSelected = true,
                Occupation = Occupation.DiabetesNurseEducator,
                QualificationId = qualifications.Skip(3).First().Id // D
            });

            await service.UpdateQualificationsPerOccupationsAsync(viewModel);

            await service.DeleteQualificationAsync(qualifications.Skip(0).First().Id); // A
            await service.DeleteQualificationAsync(qualifications.Skip(3).First().Id); // D

            var settings = await service.GetQualificationSettingsAsync();

            Assert.True(settings.QualificationsPerOccupations.ContainsKey(Occupation.Doctor));
            Assert.Contains(qualifications.Skip(1).First().Id, settings.QualificationsPerOccupations[Occupation.Doctor]);
            Assert.Contains(qualifications.Skip(2).First().Id, settings.QualificationsPerOccupations[Occupation.Doctor]);
            Assert.True(!settings.QualificationsPerOccupations.ContainsKey(Occupation.DiabetesNurseEducator));
            Assert.True(!settings.QualificationsPerOccupations.Any(x => x.Value.Contains(qualifications.Skip(3).First().Id)));
        }

        [Fact]
        public async Task DeleteQualificationAsync_ShouldThrownNotFoundException()
        {
            var service = new QualificationService(new SiteServiceMock());

            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteQualificationAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetQualificationAsync_ShouldReturnQualification()
        {
            var service = new QualificationService(new SiteServiceMock());

            await service.UpdateQualificationAsync(new QualificationViewModel() { Name = "A" });
            var qualification = (await service.GetQualificationSettingsAsync()).Qualifications.First();

            Assert.NotNull(await service.GetQualificationAsync(qualification.Id));
        }

        [Fact]
        public async Task GetQualificationAsync_ShouldThrownNotFoundException()
        {
            var service = new QualificationService(new SiteServiceMock());

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetQualificationAsync(default(Guid)));
        }
    }
}
