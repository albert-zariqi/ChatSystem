using ChatSystem.Chat.Common.Response;
using ChatSystem.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Chat.Client.Abstractions.Requests
{
    public interface IShift
    {
        Task<ResponseResult<ShiftCapacityResponse>> GetShiftCapacity(Guid shiftId, bool throwOnException = true);
    }
}
