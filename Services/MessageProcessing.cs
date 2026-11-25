using System.Text;
using System.Text.Json;
using whatsapp_cloud_api.Models;
using whatsapp_cloud_api.Services.IServices;
using static System.Net.WebRequestMethods;

namespace whatsapp_cloud_api.Services
{
    public class MessageProcessing(ILogger<MessageProcessing> logger,IWhatsAppMessage whatsAppMessage, IMpesa mpesaServices,
        IPendingPaymentStore pendingPayment,IConversationState currentState) : IMessageProcessing
    {
        private readonly string callBackUrl = "https://winning-corgi-greatly.ngrok-free.app/api/MpesaCallback/callback";
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
            var messageText = message.GetProperty("text").GetProperty("body").GetString()!.ToLower();
            logger.LogWarning($"Received text message from {fromNumber}: {messageText}");
            var profileName = contacts.GetProperty("profile").GetProperty("name").GetString();
            logger.LogWarning($"profile is {profileName}");

            if (messageText.Contains("hello") || messageText.Contains("hi") || messageText.Contains("good morning")
                || messageText.Contains("good afternoon") || messageText.Contains("good evening"))
            {
                await SendAutomatedMessage(fromNumber, $"Hello {profileName}, Welcome to Prime Hub shop");
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
                    await processListreplyMessage(message,fromNumber);
                    await SendMpesaNumberButtonsAsync(fromNumber);
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
                    header = new { type = "text", text = "Welcome to Prime Hub shop?" },
                    body = new { text = "Do you want to view our products?" },
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

        private async Task SendMpesaNumberButtonsAsync(string toPhoneNumber)
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
                    header = new { type = "text", text = "Prime Hub shop?" },
                    body = new { text = "Send STK to this number?" },
                    footer = new { text = "Please select an option" },
                    action = new
                    {
                        buttons = new[]
                        {
                        new { type = "reply", reply = new { id = "this_number", title = "Yes" } },
                        new { type = "reply", reply = new { id = "not_this", title = "No" } }

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
                                    new {id="lenovo1",title="Lenovo Thinkpad",description="Lenovo Thinkpad - Ksh.30000"},
                                    new { id = "lenovo2", title = "Lenovo", description = "Best Lenovo laptop - Ksh 25,000" },
                                    new { id = "lenovo3", title = "Lenovo Yoga", description = "Lenovo Yoga - Ksh 38,000" },
                                    
                                    
                                }
                            },
                            new
                            {
                                title="HP",
                                rows = new[]
                                {
                                    new {id="hp1",title="Hp Probook",description="Best lenovo laptop - Ksh.35000"},
                                    new {id="hp2",title="HP EliteBook",description="Best lenovo laptop - Ksh.36000"},
                                    
                                }
                            },
                            new
                            {
                                title="Lenovo",
                                rows = new[]
                                {
                                    new {id="lenovo4",title="Lenovo",description="Best lenovo laptop - Ksh.32000"}
                                }
                            },
                            new
                            {
                                title="Tecno",
                                rows = new[]
                                {
                                    new {id="techno1",title="Techno Spark 50",description="Techno Spark 50 - Ksh.20000"}
                                }
                            },
                            new
                            {
                                title="Samsung",
                                rows = new[]
                                {
                                    new {id="samsung1",title="Samsung A16",description="Samsung A16 - Ksh.26000"}
                                }
                            },
                            new
                            {
                                title="Google Pixel",
                                rows = new[]
                                {
                                    new {id="google_pixel",title="Google Pixel 7 pro",description="Google Pixel 7 pro - Ksh.36000"}
                                }
                            },
                            new
                            {
                                title="Vivo",
                                rows = new[]
                                {
                                    new {id="vivo1",title="Vivo",description="Vivo - Ksh 18,000"}
                                }
                            }

                        }
                       

                    }

                }
            };
            await whatsAppMessage.SendMessage(messagePayload);
        }

        private static readonly Dictionary<string, Decimal> Prices = new()
        {
            ["lenovo1"] = 30000,
            ["lenovo2"] = 25000,
            ["lenovo3"] = 38000,
            ["lenovo4"]=32000,
            ["hp1"] =35000,
            ["hp2"]=37000,
            ["techno1"]=20000,
            ["samsung1"]=26000,
            ["google_pixel"]=36000,
            ["vivo1"]=18000

        };


        private async Task processListreplyMessage(JsonElement message, string phoneNumber)
        {

            var listElement = message.GetProperty("interactive").GetProperty("list_reply");
            var listElementId = listElement.GetProperty("id").GetString()!;
            var productName = listElement.GetProperty("title").ToString()!;

            if(!Prices.TryGetValue(listElementId,out var price))
            {
                await SendAutomatedMessage(phoneNumber, "Unknown product!!");
                return;
            }

            var state = currentState.GetState(phoneNumber);
            state.ProductId = listElementId;
            state.Productname = productName;
            state.Amount = price;
            state.Step = ConversationStep.ConfirmingPaymentNumber;

            logger.LogWarning("Current state is: {state}", state);

            currentState.SaveState(state);

            await SendAutomatedMessage(phoneNumber, $"You've picked: {productName} its price is {price}");

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
                        await SendProductList(phoneNumber);
                       
                        break;
         
                    case "no":
                        await SendAutomatedMessage(phoneNumber,"Thank you have a good day.");
                        break;

                    case "this_number":
                        var stkResponse = await mpesaServices.InitiateSTKPushAsync(phoneNumber,2,callBackUrl);

                        logger.LogInformation("STK initiate response {stkResponse}", stkResponse);

                        using(var doc= JsonDocument.Parse(stkResponse))
                        {
                            var root = doc.RootElement;

                            if(root.TryGetProperty("ResponseCode", out var RC) && RC.GetString() == "0")
                            {
                                var checkoutId =root.GetProperty("CheckoutRequestID").GetString()!;
                                pendingPayment.Add(checkoutId, phoneNumber);
                                await SendAutomatedMessage(phoneNumber, "STK has been sent to this number. Please enter your pin");
                            }
                            else
                            {
                                await SendAutomatedMessage(phoneNumber, "Sorry, we couldn't start the payment. Please try again!!");
                            }
                        }
                        
                        break;

                    case "not_this":
                        await SendAutomatedMessage(phoneNumber, "Please enter the number you will use to make payment");
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
