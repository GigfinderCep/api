using GigFinder.Attributes;
using GigFinder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GigFinder.Controllers.Request;
using GigFinder.Utils;
using System.Data.Entity;

namespace GigFinder.Controllers
{
    public static class MessageTypes
    {
        public static string MESSAGE = "message";
        public static string AUDIO = "audio";
    }
    [RoutePrefix("api/chats")]
    [ProtectedUser]
    public class ChatsController: ApiController
    {

        private gigfinderEntities1 db = new gigfinderEntities1();

        private async Task<ChatRoom> GetChatRoom(int user1, int user2)
        {
            if(user1 == user2)
            {
                throw new Exception("you cant open a chat room with yourself");
            }
            return await db.ChatRooms
               .Where(c => (c.user_id1 == user1 && c.user_id2 == user2)
                        || (c.user_id1 == user2 && c.user_id2 == user1))
               .FirstOrDefaultAsync();
        }

        [HttpGet]
        [Route("{otherUser}/getchat")]
        public async Task<IHttpActionResult> GetUserChatRoom(int otherUser)
        {
            IHttpActionResult response;
            try
            {

                User user = UserUtils.GetCurrentUser();

                ChatRoom chatRoom = await GetChatRoom(user.id, otherUser);
    
                if (chatRoom == null)
                {
                    int firstUser = user.id < otherUser ? user.id : otherUser;
                    int secondUser = user.id > otherUser ? user.id : otherUser;
                    ChatRoom newChat = new ChatRoom
                    {
                        user_id1 = firstUser,
                        user_id2 = secondUser,
                        
                    };
                    db.ChatRooms.Add(newChat);
                    await db.SaveChangesAsync();
                    response = Ok(newChat.id);
                }
                else
                {
                    response = Ok(chatRoom.id);
                }
            }catch(Exception e)
            {
                response = BadRequest(e.ToString());
            }
                
            return response;
        }

        [HttpPost]
        [Route("{chatId}/send-message")]
        public async Task<IHttpActionResult> SendMessage(int chatId, [FromBody] RequestSendMessage request)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                User user = UserUtils.GetCurrentUser();
                ChatRoom chatRoom = await db.ChatRooms.FindAsync(chatId);
                if(chatRoom == null)
                {
                    return BadRequest("chat room not found");
                }
                if(chatRoom.user_id1 != user.id && chatRoom.user_id2 != user.id)
                {
                    return BadRequest("you are not member of this chatroom");
                }

                var msg = new Message
                {
                    chat_id = chatId,
                    sender = user.id,
                    content = request.Content,
                    date = request.Date,
                    type = MessageTypes.MESSAGE
                };
                db.Messages.Add(msg);
                await db.SaveChangesAsync();
                return Ok(msg);
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("{chatId}/messages")]
        public async Task<IHttpActionResult> GetChatMessages(int chatId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                List<Message> messages = await db.Messages
                                                .Where(m => m.chat_id == chatId)
                                                .ToListAsync();
                return Ok(messages ?? new List<Message>());
            }catch(Exception _)
            {
                return Ok(new List<Message>());
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllChats()
        {
            try
            {


                db.Configuration.LazyLoadingEnabled = false;

                if (!ModelState.IsValid)
                {
                    // Return BadRequest with validation errors
                    return BadRequest(ModelState);
                }
                User user = UserUtils.GetCurrentUser();
                List<ChatRoom> chatRooms = await db.ChatRooms.
                                Where(c => c.user_id1 == user.id || c.user_id2 == user.id)
                                .ToListAsync();
 

                return Ok(chatRooms ?? new List<ChatRoom>());
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }

        }
    }
}