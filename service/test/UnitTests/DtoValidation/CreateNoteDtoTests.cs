using Common.Core.Exceptions;
using DeliveryNoteService.Core.UseCases.Notes;
using FluentAssertions;

namespace UnitTests.DtoValidation;

public class CreateNoteDtoTests
{
    [Fact]
    public void OversizedNestedText_IsRejected()
    {
        var dto = ValidNote();
        dto = dto with { ContactInfo = dto.ContactInfo with { Contact = new string('x', 201) } };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    [Fact]
    public void MissingDetails_AreRejected()
    {
        var dto = ValidNote() with { PackageDetail = null! };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    [Fact]
    public void EmptyLines_AreRejected()
    {
        var dto = ValidNote() with { NoteLines = [] };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    [Fact]
    public void MissingNumericValue_IsRejected()
    {
        var dto = ValidNote().PackageDetail with { TareWgt = null };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    [Fact]
    public void NonPositiveQuantity_IsRejected()
    {
        var dto = ValidNote().PackageDetail with { PkgQty = 0 };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    [Fact]
    public void NegativeLineWeight_IsRejected()
    {
        var dto = ValidNote().NoteLines[0] with { PartWgt = -1 };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    [Fact]
    public void MissingDeadline_IsRejected()
    {
        var dto = ValidNote().Deadline with { ShipDate = null };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    [Fact]
    public void ReversedDeadlines_AreRejected()
    {
        var dto = ValidNote().Deadline with { ShipDate = new DateOnly(2019, 12, 31) };

        var validate = () => dto.Validate();

        validate.Should().Throw<DataValidationException>();
    }

    private static CreateDeliveryNoteDto ValidNote()
    {
        return new CreateDeliveryNoteDto
        {
            RecipientId = 1,
            TakeoverPlace = "Warehouse",
            Category = "Goods",
            ContactInfo = new CreateContactInfoDto
            {
                AssignTo = "Driver",
                AssignToPhone = "123",
                Contact = "Receiver",
                ContactPhone = "456"
            },
            PackageDetail = new CreatePackageDetailDto
            {
                TareWgt = 0,
                TareWgtUm = "kg",
                PkgQty = 1.25m,
                PkgScale = "pallet",
                PkgSizeX = 0,
                PkgSizeY = 2.5m,
                PkgSizeZ = 3,
                PkgSizeUm = "m"
            },
            Deadline = new CreateDeadlineDto
            {
                TakeoverDate = new DateOnly(2020, 1, 1),
                ShipDate = new DateOnly(2020, 1, 1),
                EstimatedArrivalDate = new DateOnly(2020, 1, 2)
            },
            NoteLines = [new CreateDeliveryNoteLineDto
            {
                PartCode = "A", PartDesc = "Part A", PartQty = 2.25m, PartQtyUm = "pcs",
                PartWgt = 0, PartWgtUm = "kg"
            }]
        };
    }
}
