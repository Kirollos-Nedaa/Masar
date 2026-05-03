(function (window) {
    const fallbackCountries = [
        { name: 'Egypt', code: '+20', iso: 'eg' }
    ];

    let countriesPromise = null;

    function loadCountries() {
        if (!countriesPromise) {
            countriesPromise = fetch('https://restcountries.com/v3.1/all?fields=name,idd,cca2')
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to load countries.');
                    }

                    return response.json();
                })
                .then(data => {
                    const countries = data
                        .filter(country => country.idd?.root && country.idd?.suffixes?.length === 1)
                        .map(country => ({
                            name: country.name.common,
                            code: `${country.idd.root}${country.idd.suffixes[0]}`,
                            iso: country.cca2.toLowerCase()
                        }))
                        .sort((left, right) => left.name.localeCompare(right.name));

                    return countries.length ? countries : fallbackCountries;
                })
                .catch(() => fallbackCountries);
        }

        return countriesPromise;
    }

    function toDigits(value) {
        return (value ?? '').replace(/\D/g, '');
    }

    function escapeHtml(value) {
        return value
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    window.MasarPhoneInput = {
        init(options) {
            const hiddenInput = document.getElementById(options.hiddenInputId);
            const localInput = document.getElementById(options.localInputId);
            const wrapper = document.getElementById(options.wrapperId);
            const countryButton = document.getElementById(options.countryButtonId);
            const dropdown = document.getElementById(options.dropdownId);
            const countryList = document.getElementById(options.countryListId);
            const countrySearch = document.getElementById(options.countrySearchId);
            const selectedFlag = document.getElementById(options.selectedFlagId);
            const selectedCode = document.getElementById(options.selectedCodeId);
            const form = document.getElementById(options.formId);

            if (!hiddenInput || !localInput || !wrapper || !countryButton || !dropdown ||
                !countryList || !countrySearch || !selectedFlag || !selectedCode || !form) {
                return null;
            }

            const minDigits = options.minDigits ?? 11;
            const requiredMessage = options.requiredMessage ?? 'Phone number is required.';
            const minDigitsMessage = options.minDigitsMessage ?? `Phone number must contain at least ${minDigits} digits.`;
            const validationMessage = document.querySelector(`[data-valmsg-for="${hiddenInput.name}"]`);

            let countries = [];
            let selectedCountry = fallbackCountries[0];

            function setValidationMessage(message) {
                if (validationMessage) {
                    validationMessage.textContent = message;
                    validationMessage.classList.toggle('field-validation-error', Boolean(message));
                    validationMessage.classList.toggle('field-validation-valid', !message);
                }

                localInput.classList.toggle('input-validation-error', Boolean(message));
                hiddenInput.classList.toggle('input-validation-error', Boolean(message));
            }

            function normalizedLocalDigits() {
                return toDigits(localInput.value).replace(/^0+/, '');
            }

            function fullPhoneValue() {
                const localDigits = normalizedLocalDigits();
                if (!localDigits) {
                    return '';
                }

                return `${selectedCountry.code} ${localDigits}`;
            }

            function syncHiddenValue() {
                hiddenInput.value = fullPhoneValue();
            }

            function phoneDigitsCount() {
                return toDigits(fullPhoneValue()).length;
            }

            function validatePhone(showMessage) {
                syncHiddenValue();

                if (!localInput.value.trim()) {
                    if (showMessage) {
                        setValidationMessage(requiredMessage);
                    }

                    return false;
                }

                if (phoneDigitsCount() < minDigits) {
                    if (showMessage) {
                        setValidationMessage(minDigitsMessage);
                    }

                    return false;
                }

                setValidationMessage('');
                return true;
            }

            function updateSelectedDisplay() {
                selectedFlag.innerHTML =
                    `<span class="iconify" data-icon="circle-flags:${selectedCountry.iso}" style="font-size:1.25rem;"></span>`;
                selectedCode.textContent = selectedCountry.code;
                window.Iconify?.scan?.(selectedFlag);
            }

            function renderCountryList(list) {
                countryList.innerHTML = list.map(country => `
                    <li class="phone-country-item" data-iso="${country.iso}">
                        <span class="iconify" data-icon="circle-flags:${country.iso}" style="font-size:1.2rem; flex-shrink:0;"></span>
                        <span>${escapeHtml(country.name)}</span>
                        <span class="phone-country-item-code">${escapeHtml(country.code)}</span>
                    </li>
                `).join('');

                countryList.querySelectorAll('.phone-country-item').forEach(item => {
                    item.addEventListener('click', () => {
                        const iso = item.getAttribute('data-iso');
                        const match = countries.find(country => country.iso === iso);
                        if (!match) {
                            return;
                        }

                        selectedCountry = match;
                        updateSelectedDisplay();
                        dropdown.style.display = 'none';
                        validatePhone(localInput.value.trim().length > 0);
                    });
                });

                window.Iconify?.scan?.(countryList);
            }

            function filterCountries(query) {
                const value = query.toLowerCase();
                renderCountryList(countries.filter(country =>
                    country.name.toLowerCase().includes(value) ||
                    country.code.includes(query)));
            }

            function hydrateFromExistingValue(phoneNumber) {
                const sortedCountries = [...countries]
                    .sort((left, right) => right.code.length - left.code.length);

                const matchedCountry = sortedCountries.find(country => phoneNumber.startsWith(country.code));
                if (!matchedCountry) {
                    syncHiddenValue();
                    return;
                }

                selectedCountry = matchedCountry;
                updateSelectedDisplay();
                localInput.value = toDigits(phoneNumber.replace(matchedCountry.code, '').trim());
                syncHiddenValue();
            }

            countryButton.addEventListener('click', event => {
                event.preventDefault();
                const isOpen = dropdown.style.display !== 'none';
                dropdown.style.display = isOpen ? 'none' : 'block';

                if (!isOpen) {
                    renderCountryList(countries);
                    countrySearch.value = '';
                    countrySearch.focus();
                }
            });

            countrySearch.addEventListener('input', event => {
                filterCountries(event.target.value);
            });

            localInput.addEventListener('input', () => {
                const digits = toDigits(localInput.value);
                if (digits !== localInput.value) {
                    localInput.value = digits;
                }

                syncHiddenValue();

                if (!localInput.value.trim()) {
                    setValidationMessage('');
                    return;
                }

                validatePhone(true);
            });

            localInput.addEventListener('blur', () => {
                validatePhone(true);
            });

            document.addEventListener('click', event => {
                if (!wrapper.contains(event.target)) {
                    dropdown.style.display = 'none';
                }
            });

            form.addEventListener('submit', event => {
                if (!validatePhone(true)) {
                    event.preventDefault();
                    localInput.focus();
                }
            });

            loadCountries().then(loadedCountries => {
                countries = loadedCountries;
                selectedCountry = countries.find(country => country.iso === (options.defaultCountryIso ?? 'eg')) ?? countries[0];
                updateSelectedDisplay();

                if (hiddenInput.value.trim()) {
                    hydrateFromExistingValue(hiddenInput.value.trim());
                }
                else {
                    syncHiddenValue();
                }
            });

            return {
                validate: () => validatePhone(true)
            };
        }
    };
})(window);
