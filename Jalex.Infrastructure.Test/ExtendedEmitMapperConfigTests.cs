using System;
using EmitMapper;
using FluentAssertions;
using Jalex.Infrastructure.Mapping;
using Xunit;

namespace Jalex.Infrastructure.Test
{
    public class ExtendedEmitMapperConfigTests
    {
        const string _soMuch = "So much?";
        const string _itSnOtEnought = "It's not enought :(";
        const string _yes = "Yes";
        const string _no = "No";

        [Fact]
        public void MappingTest()
        {
            ExtendedEmitMapperConfig<Entity, DtoEntity> extd =
                    new ExtendedEmitMapperConfig<Entity, DtoEntity>()
                        .ForMember(e => e.Money, p => p.Money > 100000 ? _soMuch : _itSnOtEnought)
                        .ForMember(e => e.Created, p => p.Created.ToShortDateString())
                        .ForMember("ChildId", p => p.Child.Id)
                        .ForMember("ChildName", p => p.Child.Name.ToUpper())
                        .ForMember("ChildIsActive", p => p.Child.IsActive ? _yes: _no);
 
            DateTime created = new DateTime(2011, 1, 1);
            Entity entity = new Entity
                                 {
                                     Id = 1,
                                     Name = "Entity1",
                                     Money = 100001,
                                     Created = created,
                                     Child = new ChildEntity
                                                 {
                                                     Id = 2,
                                                     Name = "ChildEntity1",
                                                     IsActive = true
                                                 }
                                 };

            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<Entity, DtoEntity>(extd);
            DtoEntity dto = mapper.Map(entity);

            dto.Id.Should().Be(entity.Id);
            dto.Name.Should().Be(entity.Name);
            dto.Name.Should().Be(entity.Name);
            dto.Money.Should().Be(_soMuch);
            dto.Created.Should().Be(created.ToShortDateString());
            dto.ChildId.Should().Be(entity.Child.Id);
            dto.ChildName.Should().Be(entity.Child.Name.ToUpper());
            dto.ChildIsActive.Should().Be(_yes);
        }
    }
}
