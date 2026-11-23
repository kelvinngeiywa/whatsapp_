using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Services
{
    public class ReferenceService
    {
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

            //await whatsAppMessage.SendMessage(payload);


        }

        private async Task SendListMessage(string phoneNumber)
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = phoneNumber,
                type = "interactive",
                interactive = new
                {
                    type = "list",
                    header = new { type = "text", text = "Select the food item you would like." },
                    body = new { text = "Choose from the list that has been sent to you" },
                    footer = new { text = "Thank you for shopping with us." },
                    action = new
                    {
                        button = "Order",
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
            //await whatsAppMessage.SendMessage(payload);
        }

    }
}
