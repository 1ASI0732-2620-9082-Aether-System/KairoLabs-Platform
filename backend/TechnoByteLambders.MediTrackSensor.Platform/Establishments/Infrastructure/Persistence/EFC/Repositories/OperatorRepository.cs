using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Infrastructure.Persistence.EFC.Repositories;

public class OperatorRepository(AppDbContext context)
    : BaseRepository<Operator>(context), IOperatorRepository;
