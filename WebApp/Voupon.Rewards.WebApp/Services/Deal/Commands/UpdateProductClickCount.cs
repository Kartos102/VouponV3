using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Deal.Commands
{
    public class UpdateProductClickCount : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public long MemberProfileId{ get; set; }

        private class UpdateProductClickCountHandler : IRequestHandler<UpdateProductClickCount, ApiResponseViewModel>
        {
            Voupon.Database.Postgres.RewardsEntities.RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            IAzureBlobStorage azureBlobStorage;
            public UpdateProductClickCountHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IAzureBlobStorage azureBlobStorage)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.azureBlobStorage = azureBlobStorage;
                this.vodusV2Context = vodusV2Context;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateProductClickCount request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                    if (request.MemberProfileId != 0)
                    {
                        var memberDemographicList = vodusV2Context.MemberProfileExtensions.Where(x => x.MemberProfileId == request.MemberProfileId).Select(x =>
                      new MemberProfileExtensions
                      {
                          Value = x.Value,
                          DemographicTypeId = x.DemographicTypeId,
                          DemographicValueId = x.DemographicValueId
                      }).ToList();
                        UpdateClikeCountModel updateClikeCountModel = new UpdateClikeCountModel();
                        var memberDemographicValueList = memberDemographicList.Select(x => x.Value);

                        //  Return state, Partner website idd ,and subgroup id
                        var stateId = memberDemographicList.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.State).FirstOrDefault();
                        var ageId = memberDemographicList.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Age).FirstOrDefault();
                        var genderId = memberDemographicList.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender).FirstOrDefault();
                        var ethnicityrId = memberDemographicList.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).FirstOrDefault();
                        updateClikeCountModel.DemographicAgeId = (ageId != null ? ageId.DemographicValueId.ToString() : "");
                        updateClikeCountModel.DemographicGenderId = (genderId != null ? genderId.DemographicValueId.ToString() : "");
                        updateClikeCountModel.DemographicEthnicityId = (ethnicityrId != null ? ethnicityrId.DemographicValueId.ToString() : "");
                        updateClikeCountModel.DemographicStateId = (stateId != null ? stateId.DemographicValueId : 0);
                        updateClikeCountModel.PartnerWebsiteId = 31;
                        updateClikeCountModel.PartnerId = 9;
                        if (memberDemographicList.Count() > 0)
                        {
                            var newSubgroupAddedCount = 0;
                            var newSubgroup = "";
                            foreach (var item in memberDemographicList.OrderBy(x => x.DemographicTypeId))
                            {
                                if (newSubgroupAddedCount == 4)
                                {
                                    break;
                                }
                                if (item.DemographicTypeId != (int)DemographicTypeEnum.MaritalStatus && item.DemographicTypeId != (int)DemographicTypeEnum.Ethnicity && item.DemographicTypeId != (int)DemographicTypeEnum.Age && item.DemographicTypeId != (int)DemographicTypeEnum.Gender)
                                {
                                    continue;
                                }
                                if (!string.IsNullOrEmpty(newSubgroup))
                                {
                                    newSubgroup += "_";
                                }
                                newSubgroup += item.DemographicValueId;
                                newSubgroupAddedCount++;
                            }
                            //if(newSubgroupAddedCount == 3 || newSubgroupAddedCount == 4)
                            updateClikeCountModel.SubgroupId = newSubgroup;
                        }
                        else
                        {
                            updateClikeCountModel.SubgroupId = "0";
                        }
                        var productAd = await vodusV2Context.ProductAds.Where(x => x.ProductId == request.ProductId).FirstOrDefaultAsync();
                        if (productAd != null)
                        {
                            updateClikeCountModel.ProductAdId = productAd.Id;
                        }
                        else
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Fail to get Product Reco Id";
                            return apiResponseViewModel;
                        }
                        var memberProfileProvinceId = await rewardsDBContext.Provinces.Where(x => x.DemographicId == updateClikeCountModel.DemographicStateId).Select(x => x.Id).FirstOrDefaultAsync();

                        var productToShow = new ProductAds();


                        productToShow = vodusV2Context.ProductAds.Include(x => x.ProductAdLocations).Where(x => x.Id == updateClikeCountModel.ProductAdId).FirstOrDefault();

                        UpdateClicksValues(updateClikeCountModel, productToShow, memberProfileProvinceId);

                        apiResponseViewModel.Message = "Updated Click Recomended rewards successfully";
                        apiResponseViewModel.Successful = true;
                        return apiResponseViewModel;

                        //var productAd = await vodusV2Context.ProductAds.Where(x => x.ProductId == request.ProductId).FirstOrDefaultAsync();

                        //if (productAd != null)
                        //{
                        //    productAd.AdClickCount = ++productAd.AdClickCount;
                        //    productAd.CTR = productAd.AdClickCount / productAd.AdImpressionCount;
                        //    vodusV2Context.ProductAds.Update(productAd);
                        //    await vodusV2Context.SaveChangesAsync();
                        //    apiResponseViewModel.Successful = true;
                        //    return apiResponseViewModel;
                        //}
                        //else
                        //{
                        //    apiResponseViewModel.Successful = false;
                        //    return apiResponseViewModel;
                        //}
                    }
                    else
                    {
                        //No member profile => update the product ad click count and subgroup rank 0 only
                        UpdateClikeCountModel updateClikeCountModel = new UpdateClikeCountModel();

                        var productAd = await vodusV2Context.ProductAds.Where(x => x.ProductId == request.ProductId).FirstOrDefaultAsync();
                        if (productAd != null)
                        {
                            updateClikeCountModel.ProductAdId = productAd.Id;
                        }
                        else
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Fail to get Product Reco Id";
                            return apiResponseViewModel;
                        }
                        var productToShow = new ProductAds();


                        productToShow = vodusV2Context.ProductAds.Include(x => x.ProductAdLocations).Where(x => x.Id == updateClikeCountModel.ProductAdId).FirstOrDefault();
                        updateClikeCountModel.SubgroupId = "0";
                        UpdateClicksValues(updateClikeCountModel, productToShow, 0);

                        apiResponseViewModel.Message = "Updated Click Recomended rewards successfully [1]";
                        apiResponseViewModel.Successful = true;
                        return apiResponseViewModel;
                    }
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
            }
            private void UpdateClicksValues(UpdateClikeCountModel request, ProductAds productToShow, int memberProfileProvinceId)
            {
                productToShow.AdClickCount++;
                if (productToShow.AdImpressionCount != 0)
                    productToShow.CTR = (decimal)productToShow.AdClickCount / (decimal)productToShow.AdImpressionCount;
                vodusV2Context.Update(productToShow);
                if (request.SubgroupId != null)
                {

                    // Generate all the subgroups for user based on the highest rank subgroup
                    List<string> allSubgroupsForUser = GetAllSubgroupsForUser(request.SubgroupId);

                    var productAdSubgroupList = vodusV2Context.ProductAdSubgroups.Where(x => x.ProductAdId == productToShow.Id && allSubgroupsForUser.Contains(x.SubgroupId)).ToList();
                    if (productAdSubgroupList == null || productAdSubgroupList.Count == 0)
                    {
                        foreach (var subgroup in allSubgroupsForUser)
                        {
                            ProductAdSubgroups newProductAdSubgroup = new ProductAdSubgroups()
                            {
                                SubgroupId = subgroup,
                                ProductAdId = productToShow.Id,
                                AdClickCount = 1,
                                AdImpressionCount = 1,
                                CTR = 1,
                                CreatedAt = DateTime.Now
                            };
                            vodusV2Context.Add(newProductAdSubgroup);
                        }

                    }
                    else
                    {
                        foreach (var subgroup in allSubgroupsForUser)
                        {
                            var productAdSubgroup = vodusV2Context.ProductAdSubgroups.Where(x => x.ProductAdId == productToShow.Id && x.SubgroupId == subgroup).FirstOrDefault();
                            if (productAdSubgroup != null)
                            {
                                productAdSubgroup.AdClickCount++;
                                if (productAdSubgroup.AdImpressionCount != 0)
                                    productAdSubgroup.CTR = (decimal)productAdSubgroup.AdClickCount / (decimal)productAdSubgroup.AdImpressionCount;
                                vodusV2Context.Update(productAdSubgroup);
                            }
                            else
                            {
                                ProductAdSubgroups newProductAdSubgroup = new ProductAdSubgroups()
                                {
                                    SubgroupId = subgroup,
                                    ProductAdId = productToShow.Id,
                                    AdClickCount = 1,
                                    AdImpressionCount = 1,
                                    CTR = 1,
                                    CreatedAt = DateTime.Now
                                };
                                vodusV2Context.Add(newProductAdSubgroup);
                            }
                        }
                    }
                    if (request.SubgroupId == "0")
                    {
                        vodusV2Context.SaveChanges();
                        return;
                    }
                }
                if (productToShow.ProductAdLocations.Count > 0)
                {
                    var productAdLocation = productToShow.ProductAdLocations.Where(x => x.ProvinceId == memberProfileProvinceId).FirstOrDefault();
                    if (productAdLocation != null)
                    {
                        productAdLocation.AdClickCount++;
                        if (productAdLocation.AdImpressionCount != 0)
                            productAdLocation.CTR = (decimal)productAdLocation.AdClickCount / (decimal)productAdLocation.AdImpressionCount;
                        vodusV2Context.Update(productAdLocation);
                    }
                }
                var memberProfileProductAdImpression = vodusV2Context.MemberProfileProductAdImpression.Where(x => x.ProductId == productToShow.Id && x.MemberProfileId == request.MemberProfileId).FirstOrDefault();
                if (memberProfileProductAdImpression == null)
                {
                    MemberProfileProductAdImpression newMemberProfileProductAdImpression = new MemberProfileProductAdImpression()
                    {
                        MemberProfileId = request.MemberProfileId,
                        ProductId = productToShow.Id,
                        AdClickCount = 1,
                        AdImpressionCount = 1,
                        CreatedAt = DateTime.Now,
                        CTR = 1
                    };
                    MemberProfileProductAdImpressionExtensions memberProfileProductAdImpressionExtension = new MemberProfileProductAdImpressionExtensions()
                    {
                        CreatedAt = DateTime.Now
                    };
                    newMemberProfileProductAdImpression.MemberProfileProductAdImpressionExtensions.Add(memberProfileProductAdImpressionExtension);
                    vodusV2Context.Add(newMemberProfileProductAdImpression);
                }
                else
                {
                    memberProfileProductAdImpression.AdClickCount++;
                    MemberProfileProductAdImpressionExtensions memberProfileProductAdImpressionExtension = new MemberProfileProductAdImpressionExtensions()
                    {
                        CreatedAt = DateTime.Now
                    };
                    memberProfileProductAdImpression.MemberProfileProductAdImpressionExtensions.Add(memberProfileProductAdImpressionExtension);
                    if (memberProfileProductAdImpression.AdImpressionCount != 0)
                        memberProfileProductAdImpression.CTR = (decimal)memberProfileProductAdImpression.AdClickCount / (decimal)memberProfileProductAdImpression.AdImpressionCount;
                    vodusV2Context.Update(memberProfileProductAdImpression);
                }

                if (request.PartnerId != 0)
                {
                    var productAdPartnerDomain = vodusV2Context.ProductAdPartnersDomain.Where(x => x.ProductAdId == productToShow.Id && x.PartnerId == request.PartnerId).FirstOrDefault();
                    if (productAdPartnerDomain == null)
                    {
                        ProductAdPartnersDomain productAdPartnersDomain = new ProductAdPartnersDomain()
                        {
                            PartnerId = request.PartnerId,
                            ProductAdId = productToShow.Id,
                            AdClickCount = 1,
                            AdImpressionCount = 1,
                            CTR = 1,
                            CreatedAt = DateTime.Now
                        };
                        if (request.PartnerWebsiteId != 0)
                        {
                            ProductAdPartnersDomainWebsites productAdPartnersDomainWebsite = new ProductAdPartnersDomainWebsites()
                            {
                                PartnerWebsiteId = request.PartnerWebsiteId,
                                AdClickCount = 1,
                                AdImpressionCount = 1,
                                CTR = 1,
                                CreatedAt = DateTime.Now
                            };
                            productAdPartnersDomain.ProductAdPartnersDomainWebsites.Add(productAdPartnersDomainWebsite);
                        }
                        vodusV2Context.Add(productAdPartnersDomain);

                    }
                    else
                    {
                        productAdPartnerDomain.AdClickCount++;
                        if (productAdPartnerDomain.AdImpressionCount != 0)
                            productAdPartnerDomain.CTR = (decimal)productAdPartnerDomain.AdClickCount / (decimal)productAdPartnerDomain.AdImpressionCount;
                        vodusV2Context.Update(productAdPartnerDomain);
                        if (request.PartnerWebsiteId != 0)
                        {
                            var productAdPartnerDomainWebsite = vodusV2Context.ProductAdPartnersDomainWebsites.Where(x => x.ProductAdPartnersDomainId == productAdPartnerDomain.Id && x.PartnerWebsiteId == request.PartnerWebsiteId).FirstOrDefault();

                            if (productAdPartnerDomainWebsite == null)
                            {
                                ProductAdPartnersDomainWebsites productAdPartnersDomainWebsite = new ProductAdPartnersDomainWebsites()
                                {
                                    ProductAdPartnersDomainId = productAdPartnerDomain.Id,
                                    PartnerWebsiteId = request.PartnerWebsiteId,
                                    AdClickCount = 1,
                                    AdImpressionCount = 1,
                                    CTR = 1,
                                    CreatedAt = DateTime.Now
                                };
                                vodusV2Context.Add(productAdPartnersDomainWebsite);

                            }
                            else
                            {
                                productAdPartnerDomainWebsite.AdClickCount++;
                                if(productAdPartnerDomainWebsite.AdImpressionCount!= 0)
                                productAdPartnerDomainWebsite.CTR = (decimal)productAdPartnerDomainWebsite.AdClickCount / (decimal)productAdPartnerDomainWebsite.AdImpressionCount;
                                vodusV2Context.Update(productAdPartnerDomainWebsite);
                            }
                        }

                    }
                }

                vodusV2Context.SaveChanges();
            }
            private List<string> GetAllSubgroupsForUser(string hiestRankSubgroup)
            {
                List<string> allSubgroupsForUser = new List<string>();

                var subgroupArray = hiestRankSubgroup.Split('_');
                for (int i = 0; i < subgroupArray.Count(); i++)
                {
                    var subArray = subgroupArray.Skip(i).ToArray();
                    for (int j = 0; j < subArray.Count(); j++)
                    {
                        allSubgroupsForUser.Add(GenerateSubgroup(subArray, j));
                    }
                }
                // for 3 rank subgroup
                if (subgroupArray.Count() == 3)
                {
                    allSubgroupsForUser.Add(subgroupArray[0] + "_" + subgroupArray[2]);
                }
                allSubgroupsForUser.Add("0");

                return allSubgroupsForUser;
            }

            private string GenerateSubgroup(string[] subgroup, int j)
            {
                if (j == 0)
                {
                    return subgroup[j];
                }
                else
                {
                    return GenerateSubgroup(subgroup, j - 1) + "_" + subgroup[j];
                }
            }


            public class UpdateClikeCountModel
            {
                public int? DemographicStateId { get; set; }
                public string DemographicEthnicityId { get; set; }
                public string DemographicAgeId { get; set; }
                public string DemographicGenderId { get; set; }
                public string SubgroupId { get; set; }
                //public int? DemographicstateId { get; set; }
                public long MemberProfileId { get; set; }
                public int ProductAdId { get; set; }
                public int PartnerWebsiteId { get; set; }
                public int PartnerId { get; set; }
            }
        }

    }

}
