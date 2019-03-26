import * as functions from 'firebase-functions';
import * as admin from 'firebase-admin';
import { QuerySnapshot, QueryDocumentSnapshot } from '@google-cloud/firestore';
admin.initializeApp();

interface RoomDto {
    title: string,
    description: string,
    startingPrice: number,
    thumbnail: RoomImage
}

interface RoomDetailDto {
    checkinTime: string, // iso date time string,
    checkoutTime: string,
    smokingAllowed: boolean,
    wheelChairAccessible: boolean,
    images: RoomImage [],
    reservationAgreement: string
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


interface Room {
    title: string,
    description: string,
    startingPrice: number,
    availableDate: Date | string,
    images: RoomImage [],
    thumbnail: RoomImage 
    smokingAllowed: boolean,
    wheelChairAccessible: boolean,
    reservationAgreement: string
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
        let roomDto: RoomDto = {
            description: room.description,
            title: room.title,
            startingPrice: room.startingPrice,
            thumbnail: room.thumbnail
        }
        roomDtos.push(roomDto) 
       
    });
    res.send(roomDtos);
})

export const createRoom = functions.https.onRequest(async(req, res) => {
    const roomFromBody: Room = req.body; 
    if(!roomFromBody.availableDate)
    {
        return res.send("bad request") // todo send status code etc etc
    }
    // convert string to a valid date for firebase
    roomFromBody.availableDate = new Date(roomFromBody.availableDate);
    const roomsRef = admin.firestore().collection('rooms');
    await roomsRef.add(roomFromBody);
    res.sendStatus(200);
})


// refactor into something generic for reuse or delete and use json with createroom
async function generateRoomObject(availableDateString: string ) {
    const availableDate = new Date(availableDateString);
    const room: Room = {
        availableDate,
        description: "room with a view",
        title: "2 star hotel",
        startingPrice: 150,
        smokingAllowed: false,
        reservationAgreement: "a test reservation agreement",
        images: [
            {
                imageUrl: "https://images.trvl-media.com/hotels/1000000/920000/911900/911814/37eb7948_z.jpg"
            },
            {
                imageUrl: "https://images.trvl-media.com/hotels/1000000/920000/911900/911814/d4a1d9da_z.jpg"
            },
            {
                imageUrl: "https://images.trvl-media.com/hotels/1000000/920000/911900/911814/0b52db98_z.jpg"
            },
            {
                imageUrl: "https://images.trvl-media.com/hotels/1000000/920000/911900/911814/8742452f_z.jpg"
            },
            
        
        ],
        thumbnail: {
            imageUrl: "https://images.trvl-media.com/hotels/1000000/920000/911900/911814/37eb7948_z.jpg"
        },
        wheelChairAccessible: false         
    }
    const roomsRef = admin.firestore().collection('rooms');
    await roomsRef.add(room);
}


function generateTitle() {
    
} 
