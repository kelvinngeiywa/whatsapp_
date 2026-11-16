using System.Text;
using System.Text.Json;
using whatsapp_cloud_api.Services.IServices;
using static System.Net.WebRequestMethods;

namespace whatsapp_cloud_api.Services
{
    public class MessageProcessing(ILogger<MessageProcessing> logger,IWhatsAppMessage whatsAppMessage) : IMessageProcessing
    {
        public async  Task ProcessMesssage(JsonElement message, JsonElement contact)
        {
            try
            {
                var messageType = message.GetProperty("type").GetString();
                var fromNumberRaw = contact.GetProperty("wa_id").GetString();

                var fromNumber = NormalizeWhatsAppNumber(fromNumberRaw);

                var profilName = contact.GetProperty("profile").GetProperty("name").GetString();

                switch (messageType)
                {
                    case "text":
                        await ProcessTextMessage(message, fromNumber, contact);
                        break;

                    case "interactive":
                        await ProcessInteractiveMessage(fromNumber, message,contact);
                        break;

                    case "location":
                        await SendAutomatedMessage(fromNumber, "Thank you for sharing your location");
                        break;

                    default:
                        await ProcessUnknownMessageType(messageType, fromNumber);
                        break;

                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message");
                throw;
            }
        }

        private async Task ProcessTextMessage(JsonElement message, string fromNumber, JsonElement contacts)
        {
            var messageText = message.GetProperty("text").GetProperty("body").GetString().ToLower();
            logger.LogWarning($"Received text message from {fromNumber}: {messageText}");
            var profileName = contacts.GetProperty("profile").GetProperty("name").GetString();
            logger.LogWarning($"profile is {profileName}");

            if (messageText.Contains("hello") || messageText.Contains("hi") || messageText.Contains("good morning")
                || messageText.Contains("good afternoon") || messageText.Contains("good evening"))
            {
                await SendAutomatedMessage(fromNumber, $"Hello {profileName}, Welcome to Mavuno Church");
                await SendReplyButtonsAsync(fromNumber);
            }
            else
            {
                await SendAutomatedMessage(fromNumber, $"Hello {profileName}, to get started, please send 'hello' or 'hi'");
            }
        }

        private async Task ProcessInteractiveMessage(string fromNumber,JsonElement message,JsonElement contacts)
        {
            var interactiveType = message.GetProperty("interactive").GetProperty("type").GetString();

            logger.LogWarning($"Received interactive message from {fromNumber}: {interactiveType}");
            var profileName = contacts.GetProperty("profile").GetProperty("name").GetString();

            switch (interactiveType)
            {
                case "list_reply":
                    await SendAutomatedMessage(fromNumber, "Thank you for selecting!!");
                    await processListreplyMessage(message);
                    break;
                case "button_reply":
                    await processReplyButtonMessage(message, profileName, fromNumber);

                    break;
                case "location":
                    await SendAutomatedMessage(fromNumber, "Thank you for sharing location");
                    break;
                default:
                    await ProcessUnknownMessageType(interactiveType, fromNumber);
                    break;
            }
        }

        private async Task SendAutomatedMessage(string receiptNumber, string responseMessage)
        {
            var response = new
            {
                messaging_product = "whatsapp",
                to = receiptNumber,
                type = "text",
                text = new { body = responseMessage }

            };
            await whatsAppMessage.SendMessage(response);


        }

        private async Task SendRequestLocationMessage(string phoneNumber)
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = phoneNumber,
                type = "interactive",
                interactive= new
                {
                    type="location_request_message",
                    body = new
                    {
                        text="Share location"
                    },
                    action = new
                    {
                        name="send_location"
                    }
                }
            };
            await whatsAppMessage.SendMessage(payload);
        }

        private async Task SendReplyButtonsAsync(string toPhoneNumber)
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = toPhoneNumber,
                type = "interactive",
                interactive = new
                {
                    type = "button",
                    header = new { type = "text", text = "Thank you for attending The November Gathering." },
                    body = new { text = "Would you be willing to take part in the survey?" },
                    footer = new { text = "Please select an option" },
                    action = new
                    {
                        buttons = new[]
                        {
                        new { type = "reply", reply = new { id = "yes", title = "Yes" } },
                        new { type = "reply", reply = new { id = "no", title = "No" } }
                        
                    }
                    }
                }
            };

            await whatsAppMessage.SendMessage(payload);


        }

        private async Task SendGatheringButtonsAsync(string toPhoneNumber)
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = toPhoneNumber,
                type = "interactive",
                interactive = new
                {
                    type = "button",
                    header = new { type = "text", text = "Thank you for attending The November Gathering." },
                    body = new { text = "Which days did you attend the gathering?" },
                    footer = new { text = "Please select an option" },
                    action = new
                    {
                        buttons = new[]
                        {
                        new { type = "reply", reply = new { id = "friday", title = "Friday" } },
                        new { type = "reply", reply = new { id = "saturday", title = "Saturday" } },
                        new {type="reply",reply= new {id="both",title="Both days"}}

                    }
                    }
                }
            };

            await whatsAppMessage.SendMessage(payload);


        }

        private async Task SendProductList(string mobileNumber)
        {
            var messagePayload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = mobileNumber,
                type = "interactive",
                interactive=new
                {
                    type = "list",
                    header = new { type = "text", text = "Select the food item you would like." },
                    body = new { text = "Choose from the list that has been sent to you" },
                    footer = new { text = "Thank you for shopping with us." },
                    action = new
                    {
                        button="Products",
                        sections = new[]
                        {
                            new
                            {
                                title="Computers",
                                rows = new[]
                                {
                                    new {id="1",title="Lenovo",description="Best lenovo laptop\nKsh.30000"},
                                    new { id = "comp_1", title = "Lenovo", description = "Best Lenovo laptop - Ksh 30,000" },
                                     new { id = "comp_2", title = "HP",     description = "Office laptop - Ksh 28,000" },
                                    
                                    
                                }
                            },
                            new
                            {
                                title="HP",
                                rows = new[]
                                {
                                    new {id="1",title="Lenovo",description="Best lenovo laptop\nKsh.30000"},
                                    new {id="2",title="Lenovo",description="Best lenovo laptop\nKsh.30000"},
                                    
                                }
                            },
                            new
                            {
                                title="Lenovo",
                                rows = new[]
                                {
                                    new {id="1",title="Lenovo",description="Best lenovo laptop\nKsh.30000"}
                                }
                            },
                            new
                            {
                                title="Tecno",
                                rows = new[]
                                {
                                    new {id="1",title="Lenovo",description="Best lenovo laptop\nKsh.30000"}
                                }
                            },
                            new
                            {
                                title="Samsung",
                                rows = new[]
                                {
                                    new {id="1",title="Lenovo",description="Best lenovo laptop\nKsh.30000"}
                                }
                            },
                            new
                            {
                                title="Huawei",
                                rows = new[]
                                {
                                    new {id="1",title="Lenovo",description="Best lenovo laptop\nKsh.30000"}
                                }
                            }

                        }
                       

                    }

                }
            };
            await whatsAppMessage.SendMessage(messagePayload);
        }

        private async Task SendListMessage(string phoneNumber)
        {
            var payload = new
            {
                messaging_product="whatsapp",
                recipient_type="individual",
                to=phoneNumber,
                type="interactive",
                interactive = new
                {
                    type="list",
                    header=new {type="text",text= "Select the food item you would like." },
                    body=new {text ="Choose from the list that has been sent to you"},
                    footer=new {text="Thank you for shopping with us."},
                    action = new
                    {
                        button="Order",
                        sections = new[]
                        {
                            new
                            {
                                title="Fruits",
                                rows = new[]
                                {
                                    new { id="1",title="Orange",description="qwertyy"}
                                }
                            },
                            new
                            {
                                title="vegetables",
                                rows = new[]
                                {
                                    new {id="1",title="Spinach",description="qwertyy"},
                                    new {id="2",title="Orange",description="qwertyy"},
                                    new {id="3",title="Orange",description="qwertyy"}
                                }
                            },
                            new
                            {
                                title="toiletry",
                                rows = new[]
                                {
                                    new { id="1",title="Orange",description="qwertyy"}
                                }
                            }
                        }

                    }
                }


            };
            await whatsAppMessage.SendMessage(payload);
        }

        private async Task processListreplyMessage(JsonElement message)
        {
            //var listMessage = message.TryGetProperty("interactive", out JsonElement jsonElement) &&
            //    jsonElement.TryGetProperty("list_reply", out JsonElement listElement);

            var listElement = message.GetProperty("interactive").GetProperty("list_reply").ToString();

            logger.LogWarning(listElement.ToString());
            
           
         //logger.LogWarning(message);
            

        }

        private async Task processReplyButtonMessage(JsonElement message,string username,string phoneNumber)
        {
            if(message.TryGetProperty("interactive", out JsonElement jsonElement) &&
                jsonElement.TryGetProperty("button_reply", out JsonElement buttonElement) && buttonElement.TryGetProperty("id",
                out JsonElement buttonIdElement))
            {
                var buttonIdText = buttonIdElement.ToString();

                switch (buttonIdText)
                {
                    case "yes":
                        await SendGatheringButtonsAsync(phoneNumber);
                       
                        break;
                    case "friday":
                        await SendAutomatedMessage(phoneNumber, "Ooh you attended on Friday!!");
                        break;

                    case "saturday":
                        await SendAutomatedMessage(phoneNumber, "Ooh, you attended on Saturday");
                        break;

                    case "both":
                        await SendAutomatedMessage(phoneNumber, "Ooh, you attended both days!!");
                        break;


                    case "no":
                        await SendAutomatedMessage(phoneNumber,"Thank you have a good day.");
                        break;

                    default:
                        await SendAutomatedMessage(phoneNumber, "Unknown button reply id");
                        break;

                }
            }
        }

        private async Task ProcessUnknownMessageType(string messageType, string fromNumber)
        {
            logger.LogWarning($"Received unkwown message type from {fromNumber}:{messageType}");
            await SendAutomatedMessage(fromNumber, $"Your message of type {messageType} is unknown and it cannot be processed.");
        }

        private string NormalizeWhatsAppNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                phoneNumber = "254" + phoneNumber.Substring(1);
            }

            if (phoneNumber.StartsWith("254") && phoneNumber.Length == 12)
            {
                return phoneNumber;
            }
            return phoneNumber;
        }
    }
}
