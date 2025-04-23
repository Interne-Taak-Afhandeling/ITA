using InterneTaakAfhandeling.Poller.Data;
using InterneTaakAfhandeling.Poller.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Poller.Services.NotifierState
{
    public interface INotifierStateService
    {  
        Task<InternetakenNotifierState> StartJobAsync();
        Task FinishJobAsync(ProcessingResult processingResult);
        Task<bool> TrackInternetakenAsync(Guid internetakenId, DateTimeOffset toegewezenOp);
    }
    public class ProcessingResult(bool success, Guid lastInternetakenId, DateTimeOffset lastInternetakenToegewezenOp, string? errorMessage = null)
    {
        public bool Success { get; set; } = success;
        public Guid LastInternetakenId { get; set; } = lastInternetakenId;
        public DateTimeOffset LastInternetakenToegewezenOp { get; set; } = lastInternetakenToegewezenOp;
        public string? ErrorMessage { get; set; } = errorMessage;
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
                    LastInternetakenId = Guid.Empty,
                    LastInternetakenToegewezenOp = DateTimeOffset.MinValue,
                    LastSuccessAt = null,
                    FailureCount = 0,
                    IsRunning = true,
                    LastRunAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.InternetakenNotifierStates.Add(state);
                await _dbContext.SaveChangesAsync();
            }
            state.IsRunning = true;
            state.LastRunAt = DateTime.UtcNow;
            state.UpdatedAt = DateTime.UtcNow;
            _dbContext.InternetakenNotifierStates.Update(state);
            await _dbContext.SaveChangesAsync();

            return state;
        }

        public async Task FinishJobAsync(ProcessingResult processingResult)
        {
            var state = await _dbContext.InternetakenNotifierStates.FirstAsync(x=> x.IsRunning);
            state.IsRunning = false; 
            state.UpdatedAt = DateTime.UtcNow;
            state.Remark = processingResult.ErrorMessage;
            if (!processingResult.Success)
            {
                state.FailureCount++;
            }
            else
            {
                state.LastInternetakenId = processingResult.LastInternetakenId;
                state.LastInternetakenToegewezenOp = processingResult.LastInternetakenToegewezenOp;

                state.LastSuccessAt = DateTime.UtcNow;
            }
            _dbContext.InternetakenNotifierStates.Update(state);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> TrackInternetakenAsync(Guid internetakenId, DateTimeOffset toegewezenOp)
        {
            var state = await _dbContext.InternetakenNotifierStates.FirstAsync(x => x.IsRunning);
            state.LastInternetakenId = internetakenId;
            state.LastInternetakenToegewezenOp = toegewezenOp;
            state.UpdatedAt = DateTime.UtcNow;

            _dbContext.InternetakenNotifierStates.Update(state);

          return   (await _dbContext.SaveChangesAsync()) > 0;
        }
    }
}