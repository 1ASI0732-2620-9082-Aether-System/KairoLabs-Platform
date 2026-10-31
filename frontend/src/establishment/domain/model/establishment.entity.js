/**
 * Establishment value-carrying entity within the Establishment bounded context.
 *
 * @class Establishment
 */
export class Establishment {
    /**
     * @param {Object} params - Entity attributes.
     */
    constructor({
        id = null,
        establishment_name = '',
        establishment_type = '',
        address = '',
        district = '',
        city_region = '',
        country = '',
        phone = '',
        email = '',
        website = '',
        created_at = '',
        admin_id = null,
        latitude = null,
        longitude = null,
    } = {}) {
        this.id = id;
        this.establishment_name = establishment_name;
        this.establishment_type = establishment_type;
        this.address = address;
        this.district = district;
        this.city_region = city_region;
        this.country = country;
        this.phone = phone;
        this.email = email;
        this.website = website;
        this.created_at = created_at;
        this.admin_id = admin_id;
        this.latitude = latitude;
        this.longitude = longitude;
    }
}
