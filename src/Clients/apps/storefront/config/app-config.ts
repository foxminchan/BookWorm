export const COPILOT_RUNTIME_URL = "/api/copilotkit";

export const DEFAULT_SHIPPING_COST = 5;
export const SHOP_ITEMS_PER_PAGE = 8;
export const ORDERS_PER_PAGE = 5;
export const REVIEWS_PER_PAGE = 5;

export const APP_CONFIG = {
  name: "BookWorm",
  defaultUrl: "https://bookworm.com",
  defaultLocalUrl: "http://localhost:3000",
  email: {
    support: "support@bookworm.com",
  },
  social: {
    twitter: "https://twitter.com/bookworm",
    facebook: "https://facebook.com/bookworm",
  },
} as const;
