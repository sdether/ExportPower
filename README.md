# ExportPower

## Sell excess Electrical Capacity

This mod imagines that there exists a Powergrid over which your excess capacity could be sold to 
a neighboring city. The price per MW is based on the production costs (derived from the maintenance
costs of power plants and their output rating).Since there is no statistics available about outside
economy sizes, the mod manufactures market demand by the following formula:

  - Any excess capacity up to the amount used by the city is sold at a premium
  - The next two multiples of local demand are sold at cost
  - Any excess capacity beyond that is sold at a discount

The premium and discount rates can be configured via the Mod Settings.

## Can I sell other services?

This mod is focused purely on electricity and the UI required to realistically integrate this income
source into game. If you want to sell other services, consider the excellent 
[Export Electricity Revisited](https://steamcommunity.com/sharedfiles/filedetails/?id=2727374103) mod.