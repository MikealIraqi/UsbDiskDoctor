namespace UsbDiskDoctor.Recovery.FakeCapacity;

public enum FakeCapacityStatus
{
    Genuine,
    Suspect,
    FakeCapacity,
    Inconclusive,
    Error
}

public enum FakeCapacityPhase
{
    Writing,
    Flushing,
    Reading,
    Verifying,
    Completed
}