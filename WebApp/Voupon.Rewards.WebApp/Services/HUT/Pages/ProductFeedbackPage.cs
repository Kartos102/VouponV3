using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.HUT.Pages.ProductFeedbackPage;

namespace Voupon.Rewards.WebApp.Services.HUT.Pages
{
    public class ProductFeedbackPage : IRequest<ProductFeedBackViewModel>
    {
        public int ProjectId { get; set; }
        public string Username { get; set; }
        private class ProductFeedbackPageHandler : IRequestHandler<ProductFeedbackPage, ProductFeedBackViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            public ProductFeedbackPageHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
            }

            public async Task<ProductFeedBackViewModel> Handle(ProductFeedbackPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var viewModel = new ProductFeedBackViewModel();

                try
                {
                    var hutProject = await vodusV2Context.HUTSurveyProjects.Include(x => x.HUTSurveyForms).Include(x => x.HUTSurveyParticipants).Where(x => x.Id == request.ProjectId).FirstOrDefaultAsync();

                    if (hutProject == null)
                    {
                        viewModel.ErrorMessage = "Invalid request [001]";
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    viewModel.HUTSurveyProjectId = hutProject.Id;
                    viewModel.HUTSurveyProjectName = hutProject.ExternalName;
                    viewModel.HUTSurveyProjectLanguageId = hutProject.LanguageId;
                    viewModel.CompletionReward = hutProject.VPointsReward;
                    viewModel.SurveyEndDate = hutProject.EndDate.Value;
                    viewModel.TestLength = hutProject.TestLength;

                    if (!hutProject.IsActive)
                    {
                        viewModel.ErrorMessage = $"Sorry, the project {hutProject.ExternalName} has expired";
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    if (string.IsNullOrEmpty(request.Username))
                    {
                        viewModel.ErrorMessage = "Invalid request [001.1]";
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    if (hutProject.HUTSurveyForms == null || !hutProject.HUTSurveyForms.Any())
                    {
                        viewModel.ErrorMessage = "Invalid request [002]";
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    var user = await vodusV2Context.Users.Where(x => x.UserName == request.Username).FirstOrDefaultAsync();

                    if(user == null)
                    {
                        viewModel.ErrorMessage = "Invalid request [003]";
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    var participant = hutProject.HUTSurveyParticipants.Where(x => x.Email.ToLower() == user.Email.ToLower() && x.HUTSurveyProjectId == hutProject.Id && x.IsDeleted == false).FirstOrDefault();

                    if (participant == null)
                    {
                        viewModel.ErrorMessage = "Hmm. Seems like you are not part of the qualified participants. Please check your email and register from the link received. [004] " + user.Email;
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    if (!participant.IsQualified.HasValue || !participant.UserId.HasValue || participant.StatusTypeId == 1)
                    {
                        viewModel.ErrorMessage = "Hmm. Seems like you are not part of the qualified/unconfirmed participant. Please check your email and register from the link received. [005]";
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    var userCompletedForms = await vodusV2Context.HUTSurveyResponses.Include(x => x.Participant).Where(x => x.Participant.Email == user.Email && x.HUTSurveyForm.HUTSurveyProjectId == request.ProjectId && x.Participant.IsDeleted == false).ToListAsync();

                    var completedFormIdList = userCompletedForms.Select(x => x.HUTSurveyFormId);

                    var lastFormId = hutProject.LastFormId;

                    var availableForms = hutProject.HUTSurveyForms.Where(x => !completedFormIdList.Contains(x.Id) && x.IsDeleted == false).ToList();

                    viewModel.SurveyCompleted = userCompletedForms.Count();
                    viewModel.AvailableForms =  availableForms.Count() + userCompletedForms.Count();

                    if (availableForms == null || !availableForms.Any())
                    {
                        //viewModel.ErrorMessage = "Hi, seems like there's no more question for you";
                        viewModel.IsValidRequest = true;
                        return viewModel;
                    }

                    viewModel.IsValidRequest = true;
                    viewModel.IntroMessage = hutProject.IntroMessage;

                    if (participant.CurrentFormId.HasValue)
                    {
                        var currentForm = hutProject.HUTSurveyForms.Where(x => x.Id == participant.CurrentFormId).FirstOrDefault();
                        viewModel.URL = currentForm.URL + user.Email;
                        viewModel.Name = currentForm.Name;
                        viewModel.Id = currentForm.Id;
                    }
                    else
                    {
                        //  Pick a random form 
                        var nonLastForm = availableForms.Where(x => x.Id != lastFormId).ToList();

                        if (nonLastForm != null && nonLastForm.Any())
                        {
                            if (nonLastForm.Count() == 1)
                            {
                                viewModel.URL = nonLastForm.First().URL + user.Email;
                                viewModel.Name = nonLastForm.First().Name;
                                viewModel.Id = nonLastForm.First().Id;

                            }
                            else
                            {
                                //  Pick a random form
                                if (hutProject.IsRandomized.HasValue && hutProject.IsRandomized.Value)
                                {
                                    int index = new Random().Next(nonLastForm.Count);
                                    viewModel.URL = nonLastForm.ElementAt(index).URL + user.Email;
                                    viewModel.Name = nonLastForm.ElementAt(index).Name;
                                    viewModel.Id = nonLastForm.ElementAt(index).Id;
                                }
                                else
                                {
                                    viewModel.URL = nonLastForm.First().URL;
                                    viewModel.Name = nonLastForm.First().Name;
                                    viewModel.Id = nonLastForm.First().Id;
                                }
                            }
                        }
                        else
                        {
                            viewModel.URL = availableForms.Where(x => x.Id == lastFormId).FirstOrDefault().URL + user.Email;
                            viewModel.Name = availableForms.Where(x => x.Id == lastFormId).FirstOrDefault().Name;
                            viewModel.Id = availableForms.Where(x => x.Id == lastFormId).FirstOrDefault().Id;
                        }

                        //  Update user and set current form
                        var existingParticipant = await vodusV2Context.HUTSurveyParticipants.Where(x => x.Id == participant.Id).FirstOrDefaultAsync();
                        existingParticipant.CurrentFormId = viewModel.Id;

                        vodusV2Context.HUTSurveyParticipants.Update(existingParticipant);
                        await vodusV2Context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    var lala = ex.ToString();
                    viewModel.IsValidRequest = false;
                    viewModel.ErrorMessage = "Hmm. Something went wrong. Please try again later [999]";
                }
                return viewModel;
            }
        }
        public class ProductFeedBackViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string URL { get; set; }
            public bool IsDeleted { get; set; }
            public int HUTSurveyProjectId { get; set; }
            public string HUTSurveyProjectName { get; set; }
            public int HUTSurveyProjectLanguageId{ get; set; }

            public bool IsValidRequest { get; set; }
            public string ErrorMessage { get; set; }

            public string Email { get; set; }

            public string IntroMessage { get; set; }
            public DateTime SurveyEndDate { get; set; }
            public string CompletionReward { get; set; }
            public int SurveyCompleted { get; set; }
            public int AvailableForms { get; set; }
            public int TestLength { get; set; }

        }

    }
}
