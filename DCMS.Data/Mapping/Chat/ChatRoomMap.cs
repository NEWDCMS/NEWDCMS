using DCMS.Core.Domain.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Chat
{
    public class ChatRoomMap : DCMSEntityTypeConfiguration<ChatRoom>
    {
        public override void Configure(EntityTypeBuilder<ChatRoom> builder)
        {
            builder.ToTable("ChatRooms");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    public class MessageMap : DCMSEntityTypeConfiguration<Message>
    {
        public override void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Messages");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    public class ChatUserMap : DCMSEntityTypeConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("ChatUsers");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
