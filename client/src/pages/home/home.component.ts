import { Component } from '@angular/core';
import { UiCardComponent } from "../../components/ui-card/ui-card.component";

@Component({
	selector: 'app-home',
	standalone: true,
	imports: [
		UiCardComponent,
	],
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.scss']
})
export class HomeComponent {

}