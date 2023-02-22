using System;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Chat
{
    public class ChatViewModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }

        public string UserId { get; set; }

        public ChatMessages LastMessage { get; set; }

        public DateTime? LastChat { get; set; }

        public Guid ChatGroupId { get; set; }

        public int UnreadedMessagesCount { get; set; }


    }

    public class AllChatViewModel : ChatViewModel
    {
        public ChatGroupUsers Sender { get; set; }

        public ChatGroupUsers Receiver { get; set; }
    
    }


}
