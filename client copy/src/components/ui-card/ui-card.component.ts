import { Component, HostBinding, Input, OnChanges, OnInit } from '@angular/core';

export type CardGapStyle = 'none' | 'extra-small' | 'small' | 'medium' | 'large';
export type CardAlignItems = 'flex-start' | 'flex-end' | 'center' | 'baseline';
export type CardJustifyContent = 'space-between' | 'space-evenly' | 'flex-start' | 'flex-end' | 'center';

@Component({
  selector: 'ui-card',
  standalone: true,
  templateUrl: './ui-card.component.html',
  styleUrls: ['./ui-card.component.scss']
})
export class UiCardComponent  implements OnInit, OnChanges {
  @HostBinding('class.card__flexEnd') @Input() flexEnd = false;
  @HostBinding('class.card__flexWrap') @Input() flexWrap = false;
  @HostBinding('style.alignItems') @Input() alignItems: CardAlignItems = 'flex-start';
  @HostBinding('style.justifyContent') @Input() justifyContent: CardJustifyContent = 'flex-start';

  @Input() layout: 'horizontal' | 'vertical' = 'horizontal';
  @HostBinding('class.card__vertical') vertical = false;

  @Input() gapStyle: CardGapStyle = 'small';
  @HostBinding('class.gap__extra-small') gapExtraSmall = false;
  @HostBinding('class.gap__small') gapSmall = false;
  @HostBinding('class.gap__medium') gapMedium = false;
  @HostBinding('class.gap__large') gapLarge = false;

  ngOnInit(): void {
    this.updateStyles();
  }

  ngOnChanges(): void {
    this.updateStyles();
  }

  private updateStyles(): void {
    this.vertical = this.layout === 'vertical';
    this.gapExtraSmall = this.gapStyle === 'extra-small';
    this.gapSmall = this.gapStyle === 'small';
    this.gapMedium = this.gapStyle === 'medium';
    this.gapLarge = this.gapStyle === 'large';
  }
}
