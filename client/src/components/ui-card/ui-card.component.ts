import { Component, HostBinding, Input, OnChanges, OnInit } from '@angular/core';

export type CardGapStyle = 'none' | 'extra-small' | 'small' | 'medium' | 'large';
export type CardAlignItems = 'flex-start' | 'flex-end' | 'center' | 'baseline' | 'stretch';
export type CardJustifyContent = 'space-between' | 'space-evenly' | 'space-around' | 'flex-start' | 'flex-end' | 'center';
export type CardVariant = 'default' | 'elevated' | 'outlined' | 'filled';
export type CardSize = 'small' | 'medium' | 'large';

@Component({
  selector: 'ui-card',
  standalone: true,
  templateUrl: './ui-card.component.html',
  styleUrls: ['./ui-card.component.scss']
})
export class UiCardComponent implements OnInit, OnChanges {
  // Layout Properties
  @HostBinding('class.card__flexEnd') @Input() flexEnd = false;
  @HostBinding('class.card__flexWrap') @Input() flexWrap = false;
  @HostBinding('style.alignItems') @Input() alignItems: CardAlignItems = 'flex-start';
  @HostBinding('style.justifyContent') @Input() justifyContent: CardJustifyContent = 'flex-start';

  @Input() layout: 'horizontal' | 'vertical' = 'horizontal';
  @HostBinding('class.card__vertical') vertical = false;

  // Gap Styling
  @Input() gapStyle: CardGapStyle = 'small';
  @HostBinding('class.gap__none') gapNone = false;
  @HostBinding('class.gap__extra-small') gapExtraSmall = false;
  @HostBinding('class.gap__small') gapSmall = false;
  @HostBinding('class.gap__medium') gapMedium = false;
  @HostBinding('class.gap__large') gapLarge = false;

  // Card Variants
  @Input() variant: CardVariant = 'default';
  @HostBinding('class.card__default') variantDefault = false;
  @HostBinding('class.card__elevated') variantElevated = false;
  @HostBinding('class.card__outlined') variantOutlined = false;
  @HostBinding('class.card__filled') variantFilled = false;

  // Card Size
  @Input() size: CardSize = 'medium';
  @HostBinding('class.card__small') sizeSmall = false;
  @HostBinding('class.card__medium') sizeMedium = false;
  @HostBinding('class.card__large') sizeLarge = false;

  // Interaction States
  @Input() clickable = false;
  @HostBinding('class.card__clickable') @HostBinding('attr.role') get cardRole() {
    return this.clickable ? 'button' : null;
  }
  @HostBinding('attr.tabindex') get tabIndex() {
    return this.clickable ? '0' : null;
  }

  ngOnInit(): void {
    this.updateStyles();
  }

  ngOnChanges(): void {
    this.updateStyles();
  }

  private updateStyles(): void {
    // Layout
    this.vertical = this.layout === 'vertical';
    
    // Gap
    this.resetGapClasses();
    switch (this.gapStyle) {
      case 'none':
        this.gapNone = true;
        break;
      case 'extra-small':
        this.gapExtraSmall = true;
        break;
      case 'small':
        this.gapSmall = true;
        break;
      case 'medium':
        this.gapMedium = true;
        break;
      case 'large':
        this.gapLarge = true;
        break;
    }

    // Variant
    this.resetVariantClasses();
    switch (this.variant) {
      case 'default':
        this.variantDefault = true;
        break;
      case 'elevated':
        this.variantElevated = true;
        break;
      case 'outlined':
        this.variantOutlined = true;
        break;
      case 'filled':
        this.variantFilled = true;
        break;
    }

    // Size
    this.resetSizeClasses();
    switch (this.size) {
      case 'small':
        this.sizeSmall = true;
        break;
      case 'medium':
        this.sizeMedium = true;
        break;
      case 'large':
        this.sizeLarge = true;
        break;
    }
  }

  private resetGapClasses(): void {
    this.gapNone = false;
    this.gapExtraSmall = false;
    this.gapSmall = false;
    this.gapMedium = false;
    this.gapLarge = false;
  }

  private resetVariantClasses(): void {
    this.variantDefault = false;
    this.variantElevated = false;
    this.variantOutlined = false;
    this.variantFilled = false;
  }

  private resetSizeClasses(): void {
    this.sizeSmall = false;
    this.sizeMedium = false;
    this.sizeLarge = false;
  }
}
