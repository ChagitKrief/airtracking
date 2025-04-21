namespace kriefTrackAiApi.Common.Dto
{
    public static class GraphQLQueries
    {
        public static string GetTrackedShipmentsQuery = @"
            query trackedShipments(
                $offset: NonNegativeInt!,
                $limit: PositiveInt!,
                $sort: [SortInput!],
                $orderBy: [OrderByInput!],
                $filterBy: [FilterByInput!],
                $searchTerm: String,
                $filter: EventsFilterInput,
                $options: EventsOptionsInput
            ) {
                trackedShipments(
                    offset: $offset,
                    limit: $limit,
                    sort: $sort,
                    orderBy: $orderBy,
                    filterBy: $filterBy,
                    searchTerm: $searchTerm
                ) {
                    total
                    totalFiltered
                    data {
                        id
                        metadata {
                            businessData {
                                key
                                value
                            }
                            jobNumber
                        }
                        shipment {
                            id
                            bol
                            carrier {
                                code
                                shortName
                            }
                            containerNumber
                            initialCarrierETD
                            status {
                                id
                                events(filter: $filter, options: $options) {
                                    description
                                    port {
                                        properties {
                                            locode
                                            name
                                        }
                                    }
                                    timestamps {
                                        datetime
                                        datetimeLocalized
                                        code
                                    }
                                    vessel {
                                        name
                                    }
                                    voyage
                                }
                                pol {
                                    properties {
                                        locode
                                        name
                                    }
                                }
                                currentEvent {
                                    description
                                    timestamps {
                                        datetimeLocalized
                                    }
                                    vessel {
                                        name
                                    }
                                }
                                pod {
                                    properties {
                                        locode
                                    }
                                }
                                estimatedArrivalAt
                                actualArrivalAt
                                estimatedDepartureAt
                                actualDepartureAt
                                voyageStatus
                                predicted {
                                    datetime
                                    code
                                    diffFromCarrierDays
                                }
                                milestones {
                                    type
                                    departure {
                                        timestamps {
                                            carrier {
                                                code
                                                datetime
                                            }
                                        }
                                    }
                                    utcOffset
                                }
                            }
                            initialCarrierETA
                        }
                    }
                }
            }";

        public static string GetShipmentDataQuery = @"
            query Query($shipmentGeoJsonId: ObjectId!) {
                shipmentGeoJSON(id: $shipmentGeoJsonId)
            }";

        public static string GetTrackedShipmentsByIdsQuery = @"
  query GetTrackedShipmentsByIds($ids: [ObjectId!]!) {
    trackedShipmentsByIds(ids: $ids) {
      metadata {
        businessData {
          key
          value
        }
        jobNumber
      }
      warnings {
        description
        message
      }
      shipmentId
      id
      sharedShipmentLink
      updatedAt
      createdAt
      shipment {
        id
        bol
        carrier {
          longName
          shortName
          code
        }
        containerNumber
        initialCarrierETD
        initialCarrierETA
        status {
          id
          pol {
            properties {
              locode
              name
            }
          }
          pod {
            properties {
              locode
              name
            }
          }
          estimatedArrivalAt
          estimatedDepartureAt
          actualArrivalAt
          actualDepartureAt
          voyageStatus
          predicted {
            datetime
            code
            diffFromCarrierDays
          }
          milestones {
            type
            departure {
              timestamps {
                carrier {
                  code
                  datetime
                }
              }
            }
            utcOffset
          }
          currentEvent {
            description
            timestamps {
              datetimeLocalized
            }
            vessel {
              name
            }
          }
          events {
            description
            port {
              properties {
                locode
                name
              }
            }
            timestamps {
              datetime
              datetimeLocalized
              code
            }
            vessel {
              id
              name
            }
            voyage
          }
        }
      }
    }
  }";

        public static string GetShipmentGeoJsonQuery = @"
        query Query($shipmentGeoJsonId: ObjectId!) {
      shipmentGeoJSON(id: $shipmentGeoJsonId)
    }";
    }
}






