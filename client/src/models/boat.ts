export interface IBoat {
	boatId: number;
	name: string;
	capacity: number;
	assignedUsersCount: number;
	type?: string;
	description?: string;
	location?: string;
	imageUrl?: string;
	hourlyRate?: number;
	isActive?: boolean;
}