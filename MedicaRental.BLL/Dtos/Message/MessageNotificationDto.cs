﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicaRental.BLL.Dtos.Message
{
    public record MessageNotificationDto
    (
        string Username,
        string? ProfileImage,
        string Message,
        string MessageDate
    );
}
