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
            return Ok();

        }

        [HttpGet]
        [Route("{chatId}/messages")]
        public async Task<IHttpActionResult> GetChatMessages(int chatId)
        {
            return Ok();

        }

        [HttpGet]
        [Route("all")]
        public async Task<IHttpActionResult> GetAllChats()
        {
            return Ok();

        }
    }
}