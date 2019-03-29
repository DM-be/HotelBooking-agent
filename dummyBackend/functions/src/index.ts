import * as functions from 'firebase-functions';
import * as admin from 'firebase-admin';
import { QuerySnapshot, QueryDocumentSnapshot, Timestamp } from '@google-cloud/firestore';
admin.initializeApp();

interface RoomDto {
    title: string,
    description: string,
    startingPrice: number,
    thumbnail: RoomImage,
    id: string; 
}

interface RoomDetailDto {
    checkinTime: string | Date, // iso date time string,
    checkoutTime: string | Date,
    smokingAllowed: boolean,
    wheelChairAccessible: boolean,
    roomImages: RoomImage [],
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
    reservationAgreement: string,
    checkinTime:  Timestamp,
    checkoutTime: Timestamp, 
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
            thumbnail: room.thumbnail,
            id: snapshotDoc.id
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

    const roomDetailDto: RoomDetailDto = {
        checkinTime,
        checkoutTime,
        wheelChairAccessible: room.wheelChairAccessible,
        roomImages: room.images,
        reservationAgreement: room.reservationAgreement,
        smokingAllowed: room.smokingAllowed
    }
    res.send(roomDetailDto);
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
async function generateRoomObject(availableDateString: string, checkinTime: string, checkoutTime: string ) {
    const availableDate = new Date(availableDateString);
    const checkinDate = new Date(checkinTime);
    const checkoutDate = new Date(checkoutTime);
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
        wheelChairAccessible: false,
        checkinTime: new Timestamp(checkinDate.getSeconds(), checkinDate.getMilliseconds()),
        checkoutTime: new Timestamp(checkoutDate.getSeconds(), checkoutDate.getMilliseconds()),
    }
    const roomsRef = admin.firestore().collection('rooms');
    await roomsRef.add(room);
}


function generateTitle() {
    
} 
