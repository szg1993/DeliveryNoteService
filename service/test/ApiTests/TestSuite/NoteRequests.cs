using DeliveryNoteService.Core.UseCases.Notes;

namespace ApiTests.TestSuite;

public static class NoteRequests
{
    public static CreateDeliveryNoteDto Valid(int recipientId)
        => new()
        {
            RecipientId = recipientId,
            TakeoverPlace = " Warehouse ",
            Category = " Goods ",
            ContactInfo = new CreateContactInfoDto
            {
                AssignTo = " Driver ",
                AssignToPhone = " 123 ",
                Contact = " Receiver ",
                ContactPhone = " 456 "
            },
            PackageDetail = new CreatePackageDetailDto
            {
                TareWgt = 0m,
                TareWgtUm = " kg ",
                PkgQty = 1.25m,
                PkgScale = " pallet ",
                PkgSizeX = 0m,
                PkgSizeY = 2.5m,
                PkgSizeZ = 3m,
                PkgSizeUm = " m "
            },
            Deadline = new CreateDeadlineDto
            {
                TakeoverDate = new DateOnly(2020, 1, 1),
                ShipDate = new DateOnly(2020, 1, 1),
                EstimatedArrivalDate = new DateOnly(2020, 1, 2)
            },
            NoteLines =
            [
                new CreateDeliveryNoteLineDto
                {
                    PartCode = " A ",
                    PartDesc = " Part  A ",
                    PartQty = 2.25m,
                    PartQtyUm = " pcs ",
                    PartWgt = 0m,
                    PartWgtUm = " kg "
                }
            ]
        };
}