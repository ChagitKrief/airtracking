export function showEventIconHandler (eventRecord: any){
    let imgAttr = {
      imgSrc: "",
      imgAlt: "",
    };
  
    switch (eventRecord.description) {
      case "Empty to shipper":
        imgAttr = { imgSrc: "/Empty_to_shipper.png", imgAlt: "Empty to shipper" };
        break;
      case "Gate in at first POL":
        imgAttr = { imgSrc: "/Gate_in_at_first_POL.png", imgAlt: "Gate in at first POL" };
        break;
      case "Arrival at POL":
        imgAttr = { imgSrc: "/Arrival_at_POL.png", imgAlt: "Arrival at POL" };
        break;
      case "Loaded at first POL":
        imgAttr = { imgSrc: "/loaded_at_first_POL.png", imgAlt: "Loaded at first POL" };
        break;
      case "Departure from first POL":
        imgAttr = { imgSrc: "/Departure_from_first_POL.png", imgAlt: "Departure from first POL" };
        break;
      case "Port call":
        imgAttr = { imgSrc: "/Port_call.png", imgAlt: "Port call" };
        break;
      case "Discharge at T/S port":
        imgAttr = { imgSrc: "/Discharge_at_TS_port.png", imgAlt: "Discharge at T/S port" };
        break;
      case "Arrival at T/S port":
        imgAttr = { imgSrc: "/Arrival_at_TS port.png", imgAlt: "Arrival at T/S port" };
        break;
      case "Loaded at T/S port":
        imgAttr = { imgSrc: "/Loaded_at_TS _port.png", imgAlt: "Loaded at T/S port" };
        break;
      case "Departure from T/S port":
        imgAttr = { imgSrc: "/Departure_from_TS_port.png", imgAlt: "Departure from T/S port" };
        break;
      case "Arrival at final POD":
        imgAttr = { imgSrc: "/Arrival_at_final_POD.png", imgAlt: "Arrival at final POD" };
        break;
      case "Discharge at final POD":
        imgAttr = { imgSrc: "/Discharge_at_final_POD.png", imgAlt: "Discharge at final POD" };
        break;
      case "Gate out from final POD":
        imgAttr = { imgSrc: "/Gate_out_from_final_POD.png", imgAlt: "Gate out from final POD" };
        break;
      case "Empty return to depot":
        imgAttr = { imgSrc: "/Empty_return_to_depot.png", imgAlt: "Empty return to depot" };
        break;
      case "Pickup at shipper":
        imgAttr = { imgSrc: "/Pickup_at_shipper.png", imgAlt: "Pickup at shipper" };
        break;
      case "Delivery to consignee":
        imgAttr = { imgSrc: "/Delivery_to_consignee.png", imgAlt: "Delivery to consignee" };
        break;
      case "In transshipment":
        imgAttr = { imgSrc: "/In_transshipment.png", imgAlt: "In transshipment" };
        break;
      case "Unknown":
        imgAttr = { imgSrc: "/Unknown.png", imgAlt: "Unknown" };
        break;
      default:
        imgAttr = { imgSrc: "", imgAlt: "" };
    }
  
    return imgAttr;
  };
  