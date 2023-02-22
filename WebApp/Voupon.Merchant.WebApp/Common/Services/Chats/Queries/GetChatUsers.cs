using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Chats.Queries
{
    public class GetChatUsers : IRequest<ApiResponseViewModel>
    {
        public string UserId { get; set; }
    }

    public class GetChatUsersHandler : IRequestHandler<GetChatUsers, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public GetChatUsersHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(GetChatUsers request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                //BankAccountModel bankAccountModel = new BankAccountModel();
                var messages = await rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.ChatGroupUsers.Any(y => y.UserId == request.UserId)).OrderByDescending(x => x.LastUpdatedAt).AsNoTracking().ToListAsync();

                List<ChatViewModelWithMerchant> list = new List<ChatViewModelWithMerchant>();
                if (messages != null)
                {
                    
                    foreach(var chatGroup in messages)
                    {
                        ChatViewModelWithMerchant chat = new ChatViewModelWithMerchant();
                        
                        var chatusers = chatGroup.ChatGroupUsers.ToList();
                        for (int i = chatusers.Count - 1; i >= 0; i--)
                        {
                            chat.UserName = chatusers[i].UserName;
                            chat.UserId = chatusers[i].UserId;
                            chat.ChatGroupId = chatusers[i].ChatGroupId;
                            chat.LastChat = chatusers[i].CreatedAt;

                            if (chatusers[i].UserTypeId != 1) { 
                                MerchantViewModel merchant = new MerchantViewModel();
                                merchant.MerchantName = chatusers[i].UserName;
                                merchant.MerchantLogo = chatusers[i].UserProfileImageUrl;
                                chat.Merchant = merchant;
                            }

                            /*if (chatusers[i].UserId == request.UserId)
                            {
                                chatusers.RemoveAt(i);
                                continue;
                            }

                            chatusers[i].ChatGroup = null;*/
                        }
                        
                        var lastmessages = rewardsDBContext.ChatMessages.Where(x => x.ToGroupId == chatGroup.Id).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                        //chatGroup.ChatMessages.Add(lastmessages);
                        //chatGroup.ChatGroupUsers = chatusers;
                        chat.LastMessage = lastmessages;
                        list.Add(chat);
                    }
                    response.Successful = true;
                    response.Message = "Get Messages Successfully";
                    response.Data = list;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Messages not found";
                }
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }

    public class ChatViewModelWithMerchant { 
        public int Id { get; set; }
        public string UserName { get; set; }

        public string UserId { get; set; }
        public MerchantViewModel Merchant { get; set; }

        public ChatMessages LastMessage { get; set; }

        public DateTime LastChat { get; set; }

        public Guid ChatGroupId { get; set; }


    }

    public class MerchantViewModel { 
        public int Id { get; set; }
        public string MerchantName { get; set; }

        public string MerchantLogo { get; set; }
    }
}
