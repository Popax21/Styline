module StylineChangePlayerAttributes
using ..Ahorn, Maple

@mapdef Trigger "Styline/ChangePlayerAttributes" ChangePlayerAttributes(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, applyImmediately::Bool=false, applyOnce::Bool=false,
    hairColor::String="", hairStyle::String="",
    hairAccessory::String="", hairAccessoryColor::String="",
    shirtColor::String="",
    blushColor::String=""
)

const placements = Ahorn.PlacementDict(
    "Change Player Attributes (Styline)" => Ahorn.EntityPlacement(
        ChangePlayerAttributes,
        "rectangle"
    )
)

end