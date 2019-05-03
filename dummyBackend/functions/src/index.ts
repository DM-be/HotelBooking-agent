import * as functions from 'firebase-functions';
import * as admin from 'firebase-admin';
import { QuerySnapshot, QueryDocumentSnapshot, Timestamp } from '@google-cloud/firestore';
admin.initializeApp();

interface RoomDto {
    id: string,
    title: string,
    shortDescription: string,
    description: string,
    startingPrice: number,
    thumbnail: RoomImage,
    smokingAllowed: boolean,
    wheelChairAccessible: boolean,
    capacity: number

}

interface RoomDetailDto {
    id: string,
    title: string,
    shortDescription: string,
    description: string,
    checkinTime: string | Date, // iso date time string,
    checkoutTime: string | Date,
    smokingAllowed: boolean,
    wheelChairAccessible: boolean,
    roomImages: RoomImage [],
    reservationAgreement: string,
    rates: Rate []
    lowestRate: number,
    bedDescription: string,
    squareFeet: number,
    capacity: number,
}

interface RoomImage {
    imageUrl: string
}

interface RoomRequestData {
    arrival: string,
    departure: string,
    discountCode?: string,
    room?: string,
    rate?: string,
    adults?: string,
    children?: string
}

interface Rate {
    id: string
    rateName: string, 
    price: number,
    rateDescription: string
}


interface Room {
    title: string,
    shortDescription: string, 
    description: string,
    availableDate: Date | string,
    images: RoomImage [],
    thumbnail: RoomImage 
    smokingAllowed: boolean,
    wheelChairAccessible: boolean,
    reservationAgreement: string,
    checkinTime:  Timestamp, // still needed or on hotel?
    checkoutTime: Timestamp, 
    capacity: number,
    rates: Rate [],
    bedDescription: string,
    squareFeet: number 

}

export const fetchMatchingRooms = functions.https.onRequest(async(req, res) => {
    const roomRequestData: RoomRequestData = req.body; 
    if(!roomRequestData.arrival || !roomRequestData.departure)
    {
        return res.send("bad request") // todo send status code etc etc
    }
    const arrivalDate = new Date(roomRequestData.arrival);
    const roomsRef = admin.firestore().collection('rooms');
    const query = roomsRef.where('availableDate', '<=', arrivalDate);
    const snapshot: QuerySnapshot = await query.get();
    let roomDtos: RoomDto[] = [];
    snapshot.docs.forEach((snapshotDoc: QueryDocumentSnapshot) => {
        let room = snapshotDoc.data() as Room;
        const lowestRate  = Math.min.apply(Math, room.rates.map(r => r.price));
        let roomDto: RoomDto = {
            shortDescription: room.shortDescription,
            description: room.description,
            title: room.title,
            startingPrice: lowestRate ,
            thumbnail: room.thumbnail,
            id: snapshotDoc.id,
            capacity: room.capacity,
            smokingAllowed: room.smokingAllowed,
            wheelChairAccessible: room.wheelChairAccessible
        }
        roomDtos.push(roomDto) 
    });
    res.send(roomDtos);
})


export const fetchRoomDetail = functions.https.onRequest(async(req, res) => {
    const id = req.body.id; 
    if(!id)
    {
        return res.send("bad request, need id") // todo send status code etc etc
    }
    const docRef = admin.firestore().collection('rooms').doc(id);
    const snapshot = await docRef.get();
    const room = snapshot.data() as Room;
    const checkinTime = room.checkinTime.toDate();
    const checkoutTime =  room.checkoutTime.toDate();
    const lowestRate  = Math.min.apply(Math, room.rates.map(r => r.price));

    const roomDetailDto: RoomDetailDto = {
        id,
        squareFeet: room.squareFeet,
        bedDescription: room.bedDescription,
        lowestRate,
        title: room.title,
        shortDescription: room.shortDescription,
        description: room.description,
        checkinTime,
        checkoutTime,
        wheelChairAccessible: room.wheelChairAccessible,
        roomImages: room.images,
        reservationAgreement: room.reservationAgreement,
        smokingAllowed: room.smokingAllowed,
        rates: room.rates,
        capacity: room.capacity
        
    }
    res.send(roomDetailDto);
})




export const createRoom = functions.https.onRequest(async(req, res) => {
    await generateRoomObject();
    res.sendStatus(200);
})


// refactor into something generic for reuse or delete and use json with createroom
async function generateRoomObject() {
    const availableDate = new Date();
    const checkinDate = new Date();
    const checkoutDate = new Date();
    const room: Room = {
        bedDescription: "Twin bed",
        capacity: 2,
         shortDescription: "Room comes with twin beds and is located in the center of Bruges",
         squareFeet: 22,
        availableDate,
        description: "Room in the center of Bruges, restaurants and museums in walking distance.",
        title: "Type A room",
        smokingAllowed: false,
        reservationAgreement: "a test reservation agreement",
       
        images: [
            {
                imageUrl: "https://static.cubilis.eu/securereservations/photos/hotel_de_pauw-brugge/v3/20170605hoteldepauw-8.jpg"
            },
            {
                imageUrl: "https://static.cubilis.eu/securereservations/photos/hotel_de_pauw-brugge/v3/20170605hoteldepauw-30.jpg"
            },
            {
                imageUrl: "https://static.cubilis.eu/securereservations/photos/hotel_de_pauw-brugge/v3/20170605hoteldepauw-12.jpg"
            },
      
        
        ],
        rates: [
            {
            id: "rateid123",
            price: 250,
            rateDescription: "Standard room rate, including breakfast",
            rateName: "Standard breakfast rate"
            },
            {
                id: "rateid3456",
                price: 200,
                rateDescription: "Standard room rate",
                rateName: "Standard rate"
                
            },
            {
                id: "rateid444",
                price: 180,
                rateDescription: "Not refundable after ordering",
                rateName: "Non refundable rate"
                
            }

    ],
        thumbnail: {
            imageUrl: "https://static.cubilis.eu/securereservations/photos/hotel_de_pauw-brugge/v3/20170605hoteldepauw-8.jpg"
        },
        wheelChairAccessible: false,
        checkinTime: new Timestamp(checkinDate.getUTCSeconds(), checkinDate.getUTCMilliseconds()),
        checkoutTime: new Timestamp(checkoutDate.getUTCSeconds(), checkoutDate.getUTCMilliseconds()),
    }
    const roomsRef = admin.firestore().collection('rooms');
    await roomsRef.add(room);
}


function generateTitle() {
    
} 
