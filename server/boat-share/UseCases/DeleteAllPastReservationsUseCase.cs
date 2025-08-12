using boat_share.Services;

namespace boat_share.UseCases
{
    public class DeleteAllPastReservationsUseCase
    {
        private readonly ReservationDataService _reservationDataService;
        private readonly ReservationService _reservationService;
        private readonly DeleteReservationUseCase _deleteReservationUseCase;

        public DeleteAllPastReservationsUseCase(
            ReservationDataService reservationDataService,
            ReservationService reservationService,
            DeleteReservationUseCase deleteReservationUseCase
        )
        {
            _reservationDataService = reservationDataService;
            _reservationService = reservationService;
            _deleteReservationUseCase = deleteReservationUseCase;
        }

        public async Task Execute()
        {
            var reservations = await _reservationDataService.GetAllReservationsAsync();

            foreach (var res in reservations)
            {
                if (res != null)
                {
                    var hasPast = _reservationService.HasReservationExpired(res);
                    if (hasPast)
                    {
                        await _deleteReservationUseCase.Execute(res.ReservationId);
                    }
                }
            }
        }
    }
}
