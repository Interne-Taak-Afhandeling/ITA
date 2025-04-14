using InterneTaakAfhandeling.Poller.Data;
using InterneTaakAfhandeling.Poller.Features.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Poller.Services.NotifierState
{
    public interface INotifierStateService
    {  
        Task<InternetakenNotifierState> StartJobAsync();
        Task FinishJobAsync(ProcessingResult processingResult);
    }
    public class ProcessingResult
    {
        public bool Success { get; set; }
        public Guid LastInternetakenId { get; set; }
        public string? ErrorMessage { get; set; }

        public ProcessingResult(bool success, Guid lastInternetakenId, string? errorMessage = null)
        {
            LastInternetakenId = lastInternetakenId;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }


    public class NotifierStateService : INotifierStateService
    {
        private readonly ApplicationDbContext _dbContext;

        public NotifierStateService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


      

        

        public async Task<InternetakenNotifierState> StartJobAsync()
        {
            // by default there will be only one record but just incase want to be sure that we're picking the latest
            var state = await _dbContext.InternetakenNotifierStates.OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();
            if (state == null)
            {
                state = new InternetakenNotifierState
                {
                    Id = Guid.NewGuid(),
                    LastRunAt = DateTime.MinValue.ToUniversalTime(),
                    LastInternetakenId = Guid.Empty,
                    LastSuccessAt = null,
                    FailureCount = 0,
                    IsRunning = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.InternetakenNotifierStates.Add(state);
                await _dbContext.SaveChangesAsync();
            }
            state.IsRunning = true;
            state.UpdatedAt = DateTime.UtcNow;
            _dbContext.InternetakenNotifierStates.Update(state);
            await _dbContext.SaveChangesAsync();

            return state;
        }

        public async Task FinishJobAsync(ProcessingResult processingResult)
        {
            var state = await _dbContext.InternetakenNotifierStates.FirstAsync(x=> x.IsRunning);
            state.IsRunning = false;
            state.LastRunAt = DateTime.UtcNow;
            state.LastInternetakenId = processingResult.LastInternetakenId;
            state.UpdatedAt = DateTime.UtcNow;
            state.Remark = processingResult.ErrorMessage;
            if (!processingResult.Success)
            {
                state.FailureCount++;
            }
            else
            {
                state.LastSuccessAt = DateTime.UtcNow;
            }
            _dbContext.InternetakenNotifierStates.Update(state);
            await _dbContext.SaveChangesAsync();
        }
    }
}